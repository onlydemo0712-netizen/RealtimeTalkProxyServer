using Common.DTO.PushScheduler;

using AetherCore.Repository;

namespace Repository.Interface
{
    public interface IPushSchedulerRepository : IRepository<PushSchedulerEntity>
    {
        Task<List<PushSchedulerEntity>> ClaimDuePendingAsync(int batchSize, string lockerId, TimeSpan lockTtl);
        Task<int> MarkSentAsync(IEnumerable<string> ids);
        Task<int> RescheduleDailyAsync(IEnumerable<RecurringRescheduleInfo> updates);
        Task<int> MarkFailedAsync(IEnumerable<PushFailUpdate> updates);
    }
}