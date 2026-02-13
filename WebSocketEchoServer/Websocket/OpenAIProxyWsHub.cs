using Common.DTO.ChatRole;
using Common.Setting;
using ElderAIServer.Common.Prompts;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Repository.Interface;
using System.Collections.Concurrent;
using System.Text.Json;
using AetherCore.Exceptions;
using AetherCore.WebSockets;
using AetherCore.Settings;
using AetherCore_AI.Realtime;
using ElderAIServer.Websocket.Middleware;
using ElderAIServer.Websocket.Utility;

namespace OpenAIProxyService.Websockets
{
    public static class AIRealTimeTypes
    {
        public const string Start                       = "AIRealTime.Start";                       // server -> client
        public const string Finish                      = "AIRealTime.Finish";                      // server -> client
        public const string Logging                     = "AIRealTime.Logging";                     // server -> client
        public const string Error                       = "AIRealTime.Error";                       // server -> client

        public const string Send                        = "AIRealTime.Send";                        // client -> server
        public const string InterruptReceive            = "AIRealTime.InterruptReceive";            // client -> server
        public const string RequestReply                = "AIRealTime.RequestReply";                // client -> server

        public const string ReceiveAssistantAudio       = "AIRealTime.ReceiveAssistantAudio";       // server -> client
        public const string ReceiveAssistantTextDelta   = "AIRealTime.ReceiveAssistantTextDelta";   // server -> client
        public const string ReceiveAssistantTextDone    = "AIRealTime.ReceiveAssistantTextDone";    // server -> client
        public const string ReceiveUserTextDelta        = "AIRealTime.ReceiveUserTextDelta";        // server -> client
        public const string ReceiveUserTextDone         = "AIRealTime.ReceiveUserTextDone";         // server -> client        
    }

    public sealed class RoleRuntimeConfig
    {
        public string Instructions { get; init; }   = "";
        public string Voice { get; init; }          = "";
        public bool SpeakFirst { get; init; }       = false;
    }

    public class AIMessage
    {
        public string Type { get; set; }        = string.Empty;

        // Base64 編碼的 PCM16 音訊資料 或是文字資料
        public string Payload { get; set; }     = string.Empty;
    }

    public sealed class ResponseState
    {
        // 0 = false, 1 = true
        public int HasAssistantTextDone;
    }

    /// <summary>
    /// 客戶端一連線就自動接到 OpenAI Realtime，
    /// 並把 client 的文字/音訊送給 OpenAI，把 OpenAI 的文字/音訊回來再回推給 client。
    /// </summary>
    public class OpenAIProxyWsHub : WebsocketHub
    {
        private readonly OpenAISettings _opt;
        private readonly ConcurrentDictionary<string, OpenAIRealtime> _rtByUid      = new();
        private readonly ConcurrentDictionary<string, bool> _isRespondingByUid      = new();
        private readonly ConcurrentDictionary<string, ResponseState> _stateByUid    = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DailyQuotaMiddleware _dailyQuotaMiddleware;
        private readonly ChatRoleMiddleware _charRoleMiddleware;
        private readonly ConversationSessionRecorder _conversationRecorder;

        public OpenAIProxyWsHub(IOptions<OpenAISettings> opt, 
                                IServiceScopeFactory scopeFactory, 
                                IOptions<QuotaSettings> quotaSetting,
                                IOptions<SpeakSettings> speakSetting)
        {            
            _opt                    = opt.Value;
            _scopeFactory           = scopeFactory;         // 讀取聊天對象Repository (singleton 讀取 scope)
            _dailyQuotaMiddleware   = new DailyQuotaMiddleware(scopeFactory, quotaSetting);
            _charRoleMiddleware     = new ChatRoleMiddleware(scopeFactory);
            _conversationRecorder   = new ConversationSessionRecorder(scopeFactory);

            // 加入中介層
            Use(_dailyQuotaMiddleware);
            Use(_charRoleMiddleware);
            Use(_conversationRecorder);
            Use(new SpeakSettingMiddleware(speakSetting));
        }

        /// <summary>
        /// 外部路由呼叫：註冊 client socket，建立上游 OpenAIRealtime，然後交給基底的接收迴圈（HandleText/HandleBinary）處理。
        /// </summary>
        protected override async Task OnAddCoreAsync(WebSocketIdentity identity, IReadOnlyDictionary<string, StringValues> headerDict, CancellationToken ct)
        {
            // 這邊讀取聊天對象資訊放進basicInstructions
            RoleRuntimeConfig cfg   = await GetRoleConfig(InstructionInfoParser.FromHeaders(headerDict));
            var state               = _stateByUid.GetOrAdd(identity.UserId, _ => new ResponseState());
            bool bIncludeQuota      = !cfg.SpeakFirst;

            //  建立 OpenAIRealtime（和這條 clientSocket 同壽終）
            var rt = new OpenAIRealtime(
                openAIApiKey:           _opt.ApiKey,
                model:                  _opt.Model,
                voice:                  cfg.Voice,
                basicInstructions:      cfg.Instructions,
                bAutoCreateResponse:    _opt.AutoCreate,
                bEventAsync:            false
            );

            // 1) 綁定回傳事件：把 OpenAI 的輸出回推 client
            rt.OnResposeStart       += () =>
            {
                // 開始回話
                _isRespondingByUid[identity.UserId] = true;

                // 新一輪開始：先清掉
                Interlocked.Exchange(ref state.HasAssistantTextDone, 0);

                // 回推 start
                TrySend(identity.UserId, new 
                { 
                    Type = AIRealTimeTypes.Start,
                }, ct);
            };

            rt.OnResposeFinish      += async () =>
            {
                // 結束回話
                _isRespondingByUid[identity.UserId] = false;

                // 讀取並重置（避免同一輪被算兩次）
                bool bHadAssistant = Interlocked.Exchange(ref state.HasAssistantTextDone, 0) == 1;

                if (bHadAssistant)
                {
                    if (bIncludeQuota)  // AI先說話這次不納入計算
                    {
                        // 1) +1 每日額度計數 + 1
                        _dailyQuotaMiddleware.IncDailyQuota(identity.UserName);
                    }

                    bIncludeQuota = true;

                    // 2) 重生提示詞
                    if (_dailyQuotaMiddleware.IsQuotaLow(identity.UserName))
                    {
                        var info                = FilterInfo(headerDict);
                        var newInstructions     = await BuildInstructions(identity.UserName, info);
                        rt.basicInstructions    = newInstructions;

                        // 3) 回推新的 session 資訊
                        await rt.SendSessionUpdate();
                    }
                }

                TrySend(identity.UserId, new 
                {
                    Type = AIRealTimeTypes.Finish,
                }, ct);

                // 記錄對話內容
                _conversationRecorder.FlushMessages(identity);
            };

            rt.OnUserTranscriptDelta += (txt) => 
            {
                TrySend(identity.UserId, new
                {
                    Type    = AIRealTimeTypes.ReceiveUserTextDelta,
                    Payload = txt
                }, ct);
            };

            rt.OnUserTranscriptDone += (txt) =>
            {
                TrySend(identity.UserId, new
                {
                    Type    = AIRealTimeTypes.ReceiveUserTextDone,
                    Payload = txt
                }, ct);

                // 收集使用者對話
                _conversationRecorder.AppendUserMessage(identity, txt);
            };

            rt.OnAssistantTextDone += (txt) =>
            {
                // 記錄：這輪有 AI 回覆
                Interlocked.Exchange(ref state.HasAssistantTextDone, 1);

                TrySend(identity.UserId, new
                {
                    Type    = AIRealTimeTypes.ReceiveAssistantTextDone,
                    Payload = txt
                }, ct);

                // 收集AI對話
                _conversationRecorder.AppendAIMessage(identity, txt);
            };

            rt.OnAssistantTextDelta += (txt) =>
            {
                TrySend(identity.UserId, new 
                { 
                    Type    = AIRealTimeTypes.ReceiveAssistantTextDelta, 
                    Payload = txt
                }, ct);
            };
            
            //rt.OnAssistantTextDone  += (txt) => TrySend(uid, new { type = AIRealTimeTypes.ReceiveAssistantTextDone, payload = new { text = txt } }, ct);

            // 音訊：以 binary（raw PCM16）回推給 client；若你偏好 base64，也可以改成文字訊息
            rt.OnAssistantAudioDelta    += (bytes) =>
            {
                TrySend(identity.UserId, new 
                { 
                    Type        = AIRealTimeTypes.ReceiveAssistantAudio,
                    Payload     = Convert.ToBase64String(bytes),
                }, ct);
            };

            //rt.OnAssistantAudioDone     += (_) => TrySend(uid, new { type = "assistant.audio.done" }, ct);

            // 伺服器端除錯訊息
            rt.OnLoggingDone += (msg) =>
            {
                TrySend(identity.UserId, new
                {
                    Type    = AIRealTimeTypes.Logging,
                    Payload = $"{msg}"
                }, ct);
            };

            // 錯誤訊息傳送
            rt.OnError += (errorType, errMsg) =>
            {
                try
                {
                    TrySend(identity.UserId, new 
                    { 
                        Type    = AIRealTimeTypes.Error, 
                        Payload = $"{errorType}__{errMsg}"
                    }, ct);
                }
                catch { }
            };

            // 2) 連線 OpenAI Realtime（綁定相同取消權）
            var ok = await rt.ConnectAndConfigure(ct);
            if (!ok)
            {
                // 這裡直接對「這條 clientSocket」送錯誤最準（避免 uid 競態）
                try
                {
                    await SendAsync(identity.UserId, new { Type = "error", Payload = "openai_connect_failed" }, ct);
                }
                catch { }
            }

            // 3} 判斷是否要先發話給Open AI
            if(cfg.SpeakFirst)
            {

                var tpl                 = PromptTemplateLoader.Load("care_assistant");
                var welcomeInstruction  = RoleInstructionProvider.GetInstructions(tpl, InstructionScene.Welcome);
                await rt.RequestReply(welcomeInstruction);
            }

            // 4) 設定當前 rt
            _rtByUid[identity.UserId] = rt;

            // 5) 送歡迎
            await SendAsync(identity.UserId, new { Type = "Welcome", Payload = "" }, ct);
        }

        protected override async Task OnRemoveCoreAsync(WebSocketIdentity identity, CancellationToken ct)
        {
            _isRespondingByUid.TryRemove(identity.UserId, out _);
            _stateByUid.TryRemove(identity.UserId, out _);

            if (_rtByUid.TryRemove(identity.UserId, out var rt))
            {
                try { rt.Dispose(); } catch { }
            }
        }

        private void TrySend(string uid, object obj, CancellationToken ct)
        {
            _ = SendAsync(uid, obj, ct);
        }

        // -----------------------------------------
        // Client 傳上來的訊息
        // -----------------------------------------
        protected override async Task HandleBinaryCoreAsync(WebSocketIdentity identity, byte[] bytes, CancellationToken ct)
        {
            if (!_rtByUid.TryGetValue(identity.UserId, out var rt) || rt is null || !rt.IsConnected())
            {
                TrySend(identity.UserId, new { Type = "error", Payload = "realtime_not_ready" }, ct);
                return;
            }

            // 回話中 擋掉前端麥克風音訊
            if (_isRespondingByUid.TryGetValue(identity.UserId, out var responding) && responding)
            {
                return;
            }

            var b64 = Convert.ToBase64String(bytes, 0, bytes.Length);

            await rt.SendAudioBase64Async(b64);
        }

        protected override async Task HandleTextCoreAsync(WebSocketIdentity identity, string json, CancellationToken ct)
        {
            if (!_rtByUid.TryGetValue(identity.UserId, out var rt) || rt is null || !rt.IsConnected())
            {
                TrySend(identity.UserId, new { Type = "error", Payload = "realtime_not_ready" }, ct);
                return;
            }

            AIMessage? env;
            try { env = JsonSerializer.Deserialize<AIMessage>(json); }
            catch
            {
                TrySend(identity.UserId, new { Type = "error", Payload = "invalid_json" }, ct);
                return;
            }

            if (env is null || string.IsNullOrWhiteSpace(env.Type)) return;

            switch (env?.Type)
            {
                // 要求助理生成（text + audio）
                case AIRealTimeTypes.Send:
                    {
                        if (string.IsNullOrWhiteSpace(env.Payload))
                        {
                            return;
                        }

                        await rt.SendAudioBase64Async(env?.Payload!);
                        break;
                    }
                case AIRealTimeTypes.InterruptReceive:
                    {
                        await rt.BargeInAsync(0f);
                        break;
                    }
                case AIRealTimeTypes.RequestReply:
                    {
                        await rt.CommitAndRequestResponseAsync("");
                        break;
                    }
            }
        }

        /*============================================
         * 私有方法
         * ==========================================*/
        private async Task<RoleRuntimeConfig> GetRoleConfig(InstructionInfo info)
        {
            // 這個scope只能存活在這個方法內
            using (var scope = _scopeFactory.CreateScope())
            {
                IChatRoleRepository repo    = scope.ServiceProvider.GetRequiredService<IChatRoleRepository>();

                // 預設回退值（角色不存在 / 沒填 voice 時用）
                var fallback                = new RoleRuntimeConfig
                {
                    Instructions    = _opt.BasicInstructions,
                    Voice           = _opt.Voice
                };

                try
                {
                    if (string.IsNullOrEmpty(info.roleId))
                        return fallback;

                    var entity  = await repo.GetAsync(info.roleId); // 找不到會 throw EntityNotFoundException
                    var voice   = string.IsNullOrWhiteSpace(entity.Voice) ? _opt.Voice : entity.Voice;

                    return new RoleRuntimeConfig
                    {
                        Instructions    = CombineInstructions("", entity, info),
                        Voice           = NormalizeVoice(voice),
                        SpeakFirst      = entity.SpeakFirst
                    };
                }
                catch (EntityNotFoundException)
                {
                    // 預期：角色被刪除 / roleId 無效 → 回退到預設提示詞
                    return fallback;
                }
                catch (CustomException ex)
                {
                    // 通常不是「正常情況」
                    //_logger.LogError(ex, "GetRoleConfig failed. roleId={RoleId}", roleId);
                    throw;
                }
                catch (Exception ex)
                {
                    // 非預期錯誤：建議至少 log，然後再決定要不要 throw
                    //_logger.LogError(ex, "GetRoleConfig failed. roleId={RoleId}", roleId);
                    throw;
                }
            }
        }
        private async Task<string> BuildInstructions(string userName, InstructionInfo info)
        {
            if (string.IsNullOrEmpty(info.roleId))
                return _opt.BasicInstructions;

            using var scope = _scopeFactory.CreateScope();
            var repo        = scope.ServiceProvider.GetRequiredService<IChatRoleRepository>();

            try
            {
                var entity = await repo.GetAsync(info.roleId);
                return CombineInstructions(userName, entity, info);
            }
            catch (EntityNotFoundException)
            {
                return _opt.BasicInstructions;
            }
        }

        private string CombineInstructions(string userName, ChatRoleEntity entity, InstructionInfo info)
        {
            if (entity == null)
                return _opt.BasicInstructions;

            var tpl     = PromptTemplateLoader.Load("role_default");
            var args    = new Dictionary<string, string>
            {
                ["RoleName"]            = entity.RoleName,
                ["Gender"]              = entity.IsMale ? "男性" : "女性",
                ["Description"]         = entity.Description,
                ["RoleInstructions"]    = entity.RoleInstructions,

                ["Language"]            = info.language,        // 聽的語言
                ["Locale"]              = info.locale,          // 說的語言
                ["MaxLength"]           = info.maxLength,
                ["QuotaWarnEnabled"]    = _dailyQuotaMiddleware.IsQuotaLow(userName).ToString(),

                ["BasicInstructions"]   = _opt.BasicInstructions
            };

            return PromptTemplateApplier.ApplyAndMinify(tpl, args);
        }

        private InstructionInfo FilterInfo(IReadOnlyDictionary<string, StringValues> dict)
        {
            string roleId       = dict.TryGetValue("ChatRole", out var vRoleId) ?       vRoleId.ToString()      : "";
            string language     = dict.TryGetValue("Language", out var vLanguage) ?     vLanguage.ToString()    : "繁體中文";
            string locale       = dict.TryGetValue("Locale", out var vLocale) ?         vLocale.ToString()      : "zh-TW";
            string maxLength    = dict.TryGetValue("MaxLength", out var vMaxLength) ?   vMaxLength.ToString()   : "80";

            return new InstructionInfo()
            {
                roleId      = roleId,
                language    = language,
                locale      = locale,
                maxLength   = maxLength
            };
        }

        private string NormalizeVoice(string voice)
        {
            return string.IsNullOrWhiteSpace(voice) ? _opt.Voice : voice;
        }
    }
}
