using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.PushScheduler;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Entities;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class PushSchedulerDataAccess : MongoEntityDataAccess<PushSchedulerEntity, PushSchedulerDocument>, IPushSchedulerDataAccess
    {
        private readonly IMapper _mapper;

        public PushSchedulerDataAccess(IMapper mapper)
            : base(mapper)
        {
            _mapper = mapper;
        }

        public async Task<List<PushSchedulerEntity>> ClaimDuePendingAsync(int batchSize, string lockerId, TimeSpan lockTtl)
        {
            if (batchSize <= 0) batchSize = 1;

            var nowUtc      = DateTime.UtcNow;
            var lockUntil   = nowUtc.Add(lockTtl);
            var results     = new List<PushSchedulerDocument>(batchSize);

            // 用 FindOneAndUpdate 迴圈 claim，確保「原子」且可回傳 document
            for (int i = 0; i < batchSize; i++)
            {
                var filter = Builders<PushSchedulerDocument>.Filter.And(
                    Builders<PushSchedulerDocument>.Filter.Eq(x => x.Status, PushStatus.Pending),
                    Builders<PushSchedulerDocument>.Filter.Lte(x => x.SendTime, nowUtc)
                );

                var update = Builders<PushSchedulerDocument>.Update
                    .Set(x => x.Status, PushStatus.Sending)
                    .Set(x => x.UpdatedAt, nowUtc);

                // 讓同一批裡「先到期的先處理」
                var options = new FindOneAndUpdateOptions<PushSchedulerDocument>
                {
                    ReturnDocument  = ReturnDocument.After,
                    Sort            = Builders<PushSchedulerDocument>.Sort.Ascending(x => x.SendTime)
                };

                var claimed = await DB.Collection<PushSchedulerDocument>().FindOneAndUpdateAsync(filter, update, options);

                if (claimed == null)
                    break;

                results.Add(claimed);
            }

            return MapToEntitys(results, _mapper).Result;
        }

        public async Task<int> MarkSentAsync(IEnumerable<string> ids)
        {
            var idList  = ids?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList()
                     ?? new List<string>();

            if (idList.Count == 0) return 0;

            var nowUtc  = DateTime.UtcNow;
            var filter  = Builders<PushSchedulerDocument>.Filter.In(x => x.Id, idList);
            var update  = Builders<PushSchedulerDocument>.Update
                .Set(x => x.Status, PushStatus.Sent)
                .Set(x => x.UpdatedAt, nowUtc);

            var res     = await DB.Collection<PushSchedulerDocument>().UpdateManyAsync(filter, update);
            return (int)res.ModifiedCount;
        }

        public async Task<int> RescheduleDailyAsync(IEnumerable<RecurringRescheduleInfo> updates)
        {
            var list    = updates?.Where(x => !string.IsNullOrWhiteSpace(x.Id)).ToList()
                    ?? new List<RecurringRescheduleInfo>();

            if (list.Count == 0) return 0;

            var nowUtc  = DateTime.UtcNow;
            var bulk    = new List<WriteModel<PushSchedulerDocument>>(list.Count);

            foreach (var u in list)
            {
                var filter = Builders<PushSchedulerDocument>.Filter.And(
                    Builders<PushSchedulerDocument>.Filter.Eq(x => x.Id, u.Id),
                    Builders<PushSchedulerDocument>.Filter.Eq(x => x.IsRecurring, true)
                );

                var update = Builders<PushSchedulerDocument>.Update
                    .Set(x => x.Status, PushStatus.Pending)   // 重複排程：送完回 Pending
                    .Set(x => x.SendTime, u.NextSendTimeUtc)  // 下一次送的時間
                    .Set(x => x.UpdatedAt, nowUtc);

                bulk.Add(new UpdateOneModel<PushSchedulerDocument>(filter, update));
            }

            var res = await DB.Collection<PushSchedulerDocument>()
                              .BulkWriteAsync(bulk, new BulkWriteOptions { IsOrdered = false });

            return (int)res.ModifiedCount;
        }

        public async Task<int> MarkFailedAsync(IEnumerable<PushFailUpdate> updates)
        {
            var list = updates?.ToList() ?? new List<PushFailUpdate>();
            if (list.Count == 0) return 0;

            var bulk = new List<WriteModel<PushSchedulerDocument>>(list.Count);

            foreach (var u in list)
            {
                if (string.IsNullOrWhiteSpace(u.Id))
                    continue;

                var filter = Builders<PushSchedulerDocument>.Filter.Eq(x => x.Id, u.Id);

                var update = Builders<PushSchedulerDocument>.Update
                    .Set(x => x.Status, u.NewStatus)           // Pending or Failed
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);

                bulk.Add(new UpdateOneModel<PushSchedulerDocument>(filter, update));
            }

            if (bulk.Count == 0) return 0;

            var res = await DB.Collection<PushSchedulerDocument>().BulkWriteAsync(bulk, new BulkWriteOptions { IsOrdered = false });
            return (int)res.ModifiedCount;
        }
    }
}
