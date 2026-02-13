using Common.DTO.PushScheduler;
using AetherCore.DataAccess;

namespace DataAccess.Interface
{
    public interface IPushSchedulerDataAccess : IDataAccess<PushSchedulerEntity>
    {
        Task<List<PushSchedulerEntity>> ClaimDuePendingAsync(int batchSize, string lockerId, TimeSpan lockTtl);
        Task<int> MarkSentAsync(IEnumerable<string> ids);
        Task<int> RescheduleDailyAsync(IEnumerable<RecurringRescheduleInfo> updates);
        Task<int> MarkFailedAsync(IEnumerable<PushFailUpdate> updates);
    }
}
