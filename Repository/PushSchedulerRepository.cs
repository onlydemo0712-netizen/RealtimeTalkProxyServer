using AetherCore.Repository;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.Caches;
using Common.DTO.PushScheduler;
using DataAccess.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto;
using Repository.Interface;
using System.Collections.Generic;

namespace Repository
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class PushSchedulerRepository : GenericRepository<PushSchedulerEntity, IPushSchedulerDataAccess>, IPushSchedulerRepository
    {
        public PushSchedulerRepository(IPushSchedulerDataAccess dataAccess, IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings) 
            : base(dataAccess, memoryCache, cacheSettings)
        {

        }

        public async Task<List<PushSchedulerEntity>> ClaimDuePendingAsync(int batchSize, string lockerId, TimeSpan lockTtl)
        {
            return await _dataAccess.ClaimDuePendingAsync(batchSize, lockerId, lockTtl);
        }

        public async Task<int> MarkSentAsync(IEnumerable<string> ids)
        {
            return await _dataAccess.MarkSentAsync(ids);
        }

        public async Task<int> RescheduleDailyAsync(IEnumerable<RecurringRescheduleInfo> updates)
        {
            return await _dataAccess.RescheduleDailyAsync(updates);
        }

        public async Task<int> MarkFailedAsync(IEnumerable<PushFailUpdate> updates)
        {           
            return await _dataAccess.MarkFailedAsync(updates);
        }
    }
}
