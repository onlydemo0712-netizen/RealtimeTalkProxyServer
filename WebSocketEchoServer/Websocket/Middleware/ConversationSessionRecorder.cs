using Common.DTO.Conversation;
using Common.DTO.Message;
using ElderAIServer.Websocket.Utility;
using Repository.Interface;
using Service.Interface;
using System.Collections.Concurrent;
using System.Security.Cryptography.Xml;
using System.Text;
using AetherCore.WebSockets;

namespace ElderAIServer.Websocket.Middleware
{
    public class ConversationInfo
    {
        public string UserId { get; init; }                     = string.Empty;
        public string RoleId { get; init; }                     = string.Empty;
        public RoleInfoSnapShot RoleInfo { get; init; }         = new RoleInfoSnapShot();
        public List<MessagePair> messagePairs { get; init; }    = new List<MessagePair>();
    }

    public sealed class MsgBuffer
    {
        public readonly StringBuilder User      = new();
        public readonly StringBuilder AI        = new();
        public readonly object Gate             = new();
    }

    public class ConversationSessionRecorder : IWebSocketMiddleware
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, ConversationInfo> _convationByUid = new();
        private readonly ConcurrentDictionary<string, MsgBuffer> _bufferByUid           = new();

        public ConversationSessionRecorder(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory   = scopeFactory;
        }

        /*====================================
         * 外部方法
         * ==================================*/
        public void AppendUserMessage(WebSocketIdentity identity, string userMsg)
        {
            var uid = identity.ConnectionId;

            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrEmpty(userMsg))
                return;

            if (!_convationByUid.ContainsKey(uid))
                return;

            var buf = _bufferByUid.GetOrAdd(uid, _ => new MsgBuffer());

            lock (buf.Gate)
            {
                buf.User.Append(userMsg);
            }
        }

        public void AppendAIMessage(WebSocketIdentity identity, string aiMsg)
        {
            var uid = identity.ConnectionId;

            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrEmpty(aiMsg))
                return;

            if (!_convationByUid.ContainsKey(uid))
                return;

            var buf = _bufferByUid.GetOrAdd(uid, _ => new MsgBuffer());

            lock (buf.Gate)
            {
                buf.AI.Append(aiMsg);
            }
        }

        public void FlushMessages(WebSocketIdentity identity)
        {
            var uid = identity.ConnectionId;

            if (string.IsNullOrWhiteSpace(uid))
                return;

            if (!_convationByUid.TryGetValue(uid, out var info))
                return;

            if (!_bufferByUid.TryGetValue(uid, out var buf))
                return;

            string user;
            string ai;

            lock (buf.Gate)
            {
                user    = buf.User.ToString();
                ai      = buf.AI.ToString();

                // 沒內容就不落一筆，避免空 pair
                if (string.IsNullOrWhiteSpace(user) && string.IsNullOrWhiteSpace(ai))
                    return;

                buf.User.Clear();
                buf.AI.Clear();
            }

            // messagePairs 是 List<>，多執行緒下建議鎖一下，避免同時 Flush
            lock (info.messagePairs)
            {
                info.messagePairs.Add(new MessagePair()
                {
                    UserMessage     = user,
                    AIMessage       = ai,
                    CreatedAt       = DateTime.UtcNow,
                    WarningScore    = string.IsNullOrWhiteSpace(user)? 0 : -1   // 空字串就直接給 0 分，避免後續效能浪費
                });
            }
        }

        /*=====================================
       * 實作 IWebSocketMiddleware 介面
       * =====================================*/
        public async Task OnConnectedAsync(WebSocketContext ctx, Func<Task> next)
        {
            var uid = ctx.Identity.ConnectionId;
            if (string.IsNullOrWhiteSpace(uid))
            {
                // 你可以直接拒絕或 fallback
                await next();
                return;
            }

            string chatRoleId               = ctx.Headers["ChatRole"];
            InstructionInfo instructionInfo = InstructionInfoParser.FromHeaders(ctx.Headers);

            using var scope = _scopeFactory.CreateScope();
            var repo        = scope.ServiceProvider.GetRequiredService<IChatRoleRepository>();
            var entity      = await repo.GetAsync(chatRoleId);

            ConversationInfo newInfo = new ConversationInfo()
            {
                UserId      = ctx.Identity.UserName,
                RoleId      = chatRoleId,
                RoleInfo    = new RoleInfoSnapShot()
                {
                    RoleName            = entity.RoleName,
                    Description         = entity.Description,
                    IsMale              = entity.IsMale,
                    SpeakFirst          = entity.SpeakFirst,
                    RoleInstructions    = entity.RoleInstructions,
                    Language            = instructionInfo.language,
                    Locale              = instructionInfo.locale,
                    MaxLength           = int.Parse(instructionInfo.maxLength)
                },
            };

            ConversationInfo? oldInfo = null;

            _convationByUid.AddOrUpdate(
                uid,
                addValueFactory: _ => newInfo,
                updateValueFactory: (_, existing) =>
                {
                    oldInfo = existing;        // 把舊的抓出來，等會落盤
                    return newInfo;            // 原子替換成新的 session
                });

            if (oldInfo != null)
            {
                // 把舊 session 落盤（注意要避免落盤兩次，見下段）
                await PersistAsync(oldInfo);
            }

            await next();
        }

        public async Task OnDisconnectedAsync(WebSocketContext ctx, Func<Task> next)
        {
            var uid = ctx.Identity.ConnectionId;
            if (string.IsNullOrWhiteSpace(uid))
            {
                await next();
                return;
            }

            if (_convationByUid.TryGetValue(uid, out var info))
            {
                if (_convationByUid.TryRemove(uid, out var removed))
                {
                    await PersistAsync(removed);
                }
            }

            await next();
        }

        public async Task OnTextAsync(WebSocketContext ctx, string text, Func<Task> next)
        {
            await next();
        }

        public async Task OnBinaryAsync(WebSocketContext ctx, byte[] bytes, Func<Task> next)
        {
            await next();
        }

        /*====================================
         * 內部方法
         * ==================================*/
        private async Task PersistAsync(ConversationInfo info)
        {
            using var scope         = _scopeFactory.CreateScope();
            var service             = scope.ServiceProvider.GetRequiredService<IConversationService>();
            await service.CreateConversationInfo(info.UserId, info.RoleId, info.RoleInfo, info.messagePairs);            
        }
    }
}
