using AutoMapper;
using Azure;
using Common.DTO.PushScheduler;
using Common.Setting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Org.BouncyCastle.Asn1.X509;
using Repository.Interface;
using Service.Interface;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AetherCore.Service;

namespace Service
{
    public class PushSchedulerService : GenericService<PushSchedulerEntity, PushSchedulerRequest, PushSchedulerResponse, IPushSchedulerRepository>, IPushSchedulerService
    {
        private readonly PushSchedulerSettings _pushSchedulerSettings;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PushSchedulerService(
            IPushSchedulerRepository repo, 
            IMapper mapper, 
            HttpClient httpClient, 
            IOptions<PushSchedulerSettings> opt, 
            IHttpContextAccessor httpContextAccessor) 
            : base(repo, mapper)
        {
            _httpClient             = httpClient;
            _pushSchedulerSettings  = opt.Value;
            _httpContextAccessor    = httpContextAccessor;
        }

        public override async Task<PushSchedulerResponse> CreateAsync(PushSchedulerRequest request)
        {
            var entity      = _mapper.Map<PushSchedulerEntity>(request);    // 將請求映射為 Entity
            entity.Creater  = GetCurrentUserName();
            entity          = await _repository.InsertAsync(entity);        // 實際呼叫 Repository 建立資料

            return _mapper.Map<PushSchedulerResponse>(entity);          // 將建立結果映射成回傳格式
        }

        public async Task<bool> PushMsgAllImmediately(PushMsgRequest request)
        {
            string title            = request.Title;
            string message          = request.Message;

            if(string.IsNullOrEmpty(title) && string.IsNullOrEmpty(message))
                return false;

            string appId            = _pushSchedulerSettings.AppId;
            string restApiKey       = _pushSchedulerSettings.ApiKey;

            List<string> targetIds  = new List<string>();
            targetIds.Add("All");

            return await SendPushAsync(appId, restApiKey, title, message, targetIds, null, DateTime.UtcNow.AddSeconds(3));
        }

        public async Task<bool> RunPushScheduler()
        {
            const int MaxBatchSize  = 50;      // 一次送 50 句
            var lockerId            = $"push-runner:{Environment.MachineName}";
            var lockTtl             = TimeSpan.FromMinutes(2);

            while (true)
            {
                // 1) Claim 一批到期 Pending（並改成 Sending + 鎖住）
                var dueJobs = await _repository.ClaimDuePendingAsync(MaxBatchSize, lockerId, lockTtl);
                if (dueJobs.Count == 0)
                    break;

                // 2) 逐筆送出（可以之後再優化成限速/並行）
                var sentIds             = new List<string>(dueJobs.Count);
                var recurringUpdates    = new List<RecurringRescheduleInfo>();
                var failUpdates         = new List<PushFailUpdate>();

                string appId            = _pushSchedulerSettings.AppId;
                string restApiKey       = _pushSchedulerSettings.ApiKey;
                List<string> targetIds  = new() { "All" };

                foreach (var job in dueJobs)
                {                    
                    var ok = await SendPushAsync
                        (
                            appId,
                            restApiKey,
                            job.Title,
                            job.Message,
                            targetIds,
                            null,
                            DateTime.UtcNow.AddSeconds(3)
                        );

                    if (!ok)
                    {
                        failUpdates.Add(new PushFailUpdate(
                            Id: job.Id,
                            NewStatus: PushStatus.Failed,
                            Error: "OneSignal send failed",
                            LastTryAtUtc: DateTime.UtcNow));
                    }

                    if (job.IsRecurring)
                    {
                        // 延後一天
                        recurringUpdates.Add(new RecurringRescheduleInfo(job.Id, job.SendTime.AddHours(24)));
                    }
                    else
                    {
                        sentIds.Add(job.Id);
                    }
                }

                // 3) Bulk 寫回成功
                if (sentIds.Count > 0)
                    await _repository.MarkSentAsync(sentIds);

                // 4) 重複 → 改回 Pending + 更新 SendTime
                if (recurringUpdates.Count > 0)
                    await _repository.RescheduleDailyAsync(recurringUpdates);

                // 5) Bulk 寫回失敗（含 TryCount++，Pending / Failed）
                if (failUpdates.Count > 0)
                    await _repository.MarkFailedAsync(failUpdates);
            }

            return true;
        }

        /*===========================================
         * 內部方法
         * =========================================*/
        private async Task<bool> SendPushAsync(
                string appId,
                string restApiKey,
                string title,
                string message,
                List<string> targetIds,
                Dictionary<string, object>? data = null,
                DateTime? sendAfterUtc = null)
        {
            var payload = new
            {
                app_id              = appId,

                included_segments   = targetIds.ToArray(),

                headings            = new
                {
                    en              = title,
                    zh_Hant         = title
                },

                contents            = new
                {
                    en              = message,
                    zh_Hant         = message
                },

                data                = data,
                send_after          = sendAfterUtc?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            var json                        = JsonSerializer.Serialize(payload, new JsonSerializerOptions { IgnoreNullValues = true });
            var request                     = new HttpRequestMessage(HttpMethod.Post, "https://api.onesignal.com/notifications");
            request.Headers.Authorization   = new AuthenticationHeaderValue("Basic", restApiKey);
            request.Content                 = new StringContent(json, Encoding.UTF8, "application/json");
            var response                    = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OneSignal Error: {error}");
                return false;
            }

            return true;
        }

        private string GetCurrentUserName()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                return "System";

            return user.Identity.Name ?? "Unknown";
        }
    }
}
