using AetherCore.DTO;
using AetherCore.Module.Interface;
using AetherCore.Service;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Jobs;
using Common.DTO.Message;
using Common.Setting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Repository.Interface;
using Service.Interface;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class JobsService : GenericService<JobsEntity, JobsRequest, JobsResponse, IJobsRepository>, IJobsService
    {
        private static readonly HashSet<string> LifeSafetyCategories = new(StringComparer.OrdinalIgnoreCase)
                                                                        {
                                                                            "self-harm",
                                                                            "suicide",
                                                                            "self_harm",
                                                                            "suicidal_ideation",
                                                                            "self_harm_intent",
                                                                            "self_harm_instruction"
                                                                        };

        private readonly IModerationClient _moderationClient;
        private readonly IMessageRepository _messageRepository;
        private readonly ISmtpEmailSender _smtpEmailSender;
        private readonly MailReceiveSettings _mailReceiveSetting;

        public JobsService(IJobsRepository repo, 
                            IMapper mapper, 
                            IModerationClient moderationClient, 
                            IMessageRepository messageRepository,
                            ISmtpEmailSender smtpEmailSender,
                            IOptions<MailReceiveSettings> opt) 
            : base(repo, mapper)
        {
            _moderationClient   = moderationClient;
            _messageRepository  = messageRepository;
            _smtpEmailSender    = smtpEmailSender;
            _mailReceiveSetting = opt.Value;
        }

        public async Task<bool> DailyCheck()
        {
            const int batchSize         = 100;      // 一次送 100 句
            const int NotifyThreshold   = 80;       // 例如 80 分以上寄信
            var updatedCount            = 0;

            while (true)
            {
                // 1) 撈未評分
                var unscoredMessages = await _messageRepository.FindUnscoredAsync(batchSize);
                if (unscoredMessages.Count == 0)
                    break;

                // 2) 組合送審內容
                List<ModerationInput> messages = unscoredMessages
                        .Where(m => !string.IsNullOrWhiteSpace(m.Pair?.UserMessage))
                        .Select(m => new ModerationInput
                        {
                            MessageId   = m.Id,
                            Content     = m.Pair.UserMessage
                        })
                        .ToList();

                // 3) 送交給AI判斷 並給予警示評分
                List<ModerationResponse> moderationResult   = await _moderationClient.CheckBatchAsync(messages);
                Dictionary<string, int> scoreMap            = moderationResult.Where(r => r.Results != null)
                                                               .SelectMany(r => r.Results)
                                                               .ToDictionary(
                                                                   x => x.MessageId,
                                                                   x => ComputeWarningScore(x.Result)
                                                               );

                // 4) 組成「要寫回 DB」的更新清單（只留 Id + Score）
                var updates = unscoredMessages.Select(m =>
                {
                    var score       = scoreMap.TryGetValue(m.Id, out var s) ? s : 0;
                    var msgContent  = m.Pair?.UserMessage ?? string.Empty;
                    var msgTime     = m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty;

                    return new WarningScoreUpdate(m.Id, score, msgContent, msgTime);
                }).ToList();

                // 5) Bulk 更新回資料庫（你要的部分）
                var modified = await _messageRepository.UpdateWarningScoresAsync(updates);
                updatedCount += modified;

                // 6) 挑出高分寄信
                var toNotify = updates.Where(x => x.WarningScore >= NotifyThreshold).ToList();
                await SendMail(toNotify);
            }

            return true;
        }

        private async Task SendMail(List<WarningScoreUpdate> toNotifys)
        {
            if (toNotifys == null || toNotifys.Count == 0)
                return;

            foreach (var item in toNotifys)
            {
                string subject          = $"{_mailReceiveSetting.Subject}_{item.MessageId}";
                string textBodyBasic    = $"\n\nMessage ID: {item.MessageId}\n Message Time: {item.MsgTime}\nWarning Score: {item.WarningScore}\nContent: {item.MsgContent}";

                await _smtpEmailSender.SendAsync(_mailReceiveSetting.ToMail, subject, textBodyBasic);
            }            
        }

        private int ComputeWarningScore(ModerationCheckResult r)
        {
            if (r?.CategoryScores == null || r.CategoryScores.Count == 0)
                return 0;


            // 只留下「生命安全」相關分類
            var lifeSafetyScores = r.CategoryScores
                .Where(kv => LifeSafetyCategories.Contains(kv.Key))
                .Select(kv => kv.Value) // 0 ~ 1
                .ToList();

            if (lifeSafetyScores.Count == 0)
                return 0;

            var max01   = lifeSafetyScores.Max();
            max01       = max01 * 5.0f + 0.2f;       // 測試用的正規化處理
            var score   = (int)Math.Round(max01 * 100.0, MidpointRounding.AwayFromZero);

            return Math.Clamp(score, 0, 100);
        }
    }
}
