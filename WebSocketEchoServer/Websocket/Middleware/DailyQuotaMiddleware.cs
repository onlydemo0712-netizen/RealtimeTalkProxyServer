using Common.Setting;
using Microsoft.Extensions.Options;
using Repository.Interface;
using System.Collections.Concurrent;
using AetherCore.WebSockets;

namespace ElderAIServer.Websocket.Middleware
{
    public sealed class DailyQuotaState
    {
        public string DayKey { get; set; }          = "";
        public int Count { get; set; }
        public bool IsAlreadyNotify { get; set; }   = false;
    }

    public sealed class DailyQuotaMiddleware : IWebSocketMiddleware
    {
        private readonly ConcurrentDictionary<string, DailyQuotaState> _counterByUid = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeZoneInfo _twTz;
        private int _dailyLimit;

        public DailyQuotaMiddleware(IServiceScopeFactory scopeFactory, IOptions<QuotaSettings> quotaOpt)
        {
            _scopeFactory   = scopeFactory;
            _dailyLimit     = quotaOpt.Value.DailySentenceLimit;
            _twTz           = GetTaiwanTimeZone();
        }

        /*=====================================
        * 實作 IWebSocketMiddleware 介面
        * =====================================*/
        public async Task OnConnectedAsync(WebSocketContext ctx, Func<Task> next)
        {
            await InitDailyQuota(ctx.Identity.UserName, ctx.CancellationToken);
            await ctx.SendJsonAsync(CreateInitMessage(ctx.Identity.UserName)); // for init message
            await next();
        }

        public async Task OnDisconnectedAsync(WebSocketContext ctx, Func<Task> next)
        {
            await FlushDailyQuotaToDbAsync(ctx.Identity.UserName, ctx.CancellationToken);
            await next();
        }

        public async Task OnTextAsync(WebSocketContext ctx, string text, Func<Task> next)
        {
            await next();
        }

        public async Task OnBinaryAsync(WebSocketContext ctx, byte[] bytes, Func<Task> next)
        {
            string userName = ctx.Identity.UserName;

            if (IsOverQuota(userName) && NeedToNotify(userName))
            {
                // 發送超過額度訊息
                await ctx.SendJsonAsync(CreateOverQuotaMessage(userName));

                if (_counterByUid.TryGetValue(userName, out var state))
                {
                    state.IsAlreadyNotify = true;
                }

                return;
            }

            await next();
        }
        /*============================================
         * 外部調用
         * ==========================================*/
        public void IncDailyQuota(string userName)
        {
            string todayKey = GetDayKeyTw();

            _counterByUid.AddOrUpdate(
                userName,
                _ => new DailyQuotaState { DayKey = todayKey, Count = 1, IsAlreadyNotify = false },
                (_, old) =>
                {
                    if (!string.Equals(old.DayKey, todayKey, StringComparison.Ordinal))// 跨日重置
                        return new DailyQuotaState { DayKey = todayKey, Count = 1, IsAlreadyNotify = false };

                    return new DailyQuotaState { DayKey = old.DayKey, Count = old.Count + 1 };
                }
            );
        }

        public bool IsQuotaLow(string userName)
        {
            if (_counterByUid.TryGetValue(userName, out var state))
            {
                return _dailyLimit - state.Count <= 5;
            }

            return false;
        }

        public bool IsOverQuota(string userName)
        {
            if (_counterByUid.TryGetValue(userName, out var state))
            {
                return _dailyLimit <= state.Count;
            }

            return false;
        }

        public bool NeedToNotify(string userName)
        {
            if (_counterByUid.TryGetValue(userName, out var state))
            {
                return !state.IsAlreadyNotify;
            }

            return false;
        }

        /*============================================
         * 私有方法 (DailyQuota)
         * ==========================================*/
        private async Task InitDailyQuota(string? userName, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(userName))
                return;

            string todayKey = GetDayKeyTw();

            // 有快取且是今天，就不用 init
            // 因為daily limit 一定要拿 所以就無視快取
            //if (_counterByUid.TryGetValue(userName, out var state) && state.DayKey == todayKey)
                //return;

            using var scope = _scopeFactory.CreateScope();
            var repo        = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var entity      = await repo.GetAsync(userName);

            // 從DB取出正確的資料
            _dailyLimit     = entity.DailyLimit;
            int dailyQuota  = 0;
            entity.DailySentenceCounts?.TryGetValue(todayKey, out dailyQuota);

            // 直接覆蓋成今天（因為跨日就應該重置）
            _counterByUid.AddOrUpdate(
                userName,
                _ => new DailyQuotaState { DayKey = todayKey, Count = dailyQuota, IsAlreadyNotify = false },
                (_, old) => new DailyQuotaState { DayKey = todayKey, Count = dailyQuota }
            );
        }

        private async Task FlushDailyQuotaToDbAsync(string userName, CancellationToken ct)
        {
            // 1) 先從記憶體拿狀態（沒有就不用寫）
            if (!_counterByUid.TryGetValue(userName, out var quota))
                return;

            // 2) 基本防呆
            if (string.IsNullOrWhiteSpace(quota.DayKey))
                return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo        = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var entity      = await repo.GetAsync(userName);

                // 3) 寫回 entity（依你 DB 結構調整）
                entity.DailySentenceCounts                  ??= new Dictionary<string, int>();
                entity.DailySentenceCounts[quota.DayKey]    = quota.Count;

                await repo.UpdateAsync(userName, entity);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不處理
            }
            catch (Exception ex)
            {
                // _logger?.LogError(ex, "FlushDailyQuotaToDbAsync failed. uid={Uid}, dayKey={DayKey}, count={Count}", uid, quota.DayKey, quota.Count);
            }
        }

        private TimeZoneInfo GetTaiwanTimeZone()
        {
            // Windows: "Taipei Standard Time"
            // Linux:   "Asia/Taipei"
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"); }
        }

        private string GetDayKeyTw()
        {
            var twNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _twTz);
            return twNow.ToString("yyyyMMdd");
        }

        private object? CreateInitMessage(string? userName)
        {
            if (string.IsNullOrEmpty(userName))
                return new {Type = "QuotaInit" };

            var used    = _counterByUid.TryGetValue(userName, out var state) ? state.Count : 0;

            return new
            {
                Type    = "QuotaInit",
                Payload = new
                {
                    userName,
                    quota = new
                    {
                        dailyLimit  = _dailyLimit,
                        used,
                        remaining   = Math.Max(0, _dailyLimit - used)
                    },
                }
            };
        }

        private object CreateOverQuotaMessage(string userName)
        {
            return new
            {
                Type    = "OverQuota",
                Payload = new
                {
                    userName,
                }
            };
        }
    }
}
