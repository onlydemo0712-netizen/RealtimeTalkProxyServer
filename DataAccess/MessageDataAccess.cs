using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Message;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Entities;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class MessageDataAccess : MongoEntityDataAccess<MessageEntity, MessageDocument>, IMessageDataAccess
    {
        private readonly IMapper _mapper;

        public MessageDataAccess(IMapper mapper)
            : base(mapper)
        {
            _mapper = mapper;
        }

        public async Task<List<MessageEntity>> GetDetail(string conversationId)
        {
            var docs        = await DB.Find<MessageDocument>().Match(d => d.ConversationId == conversationId).ExecuteAsync();
            var entities    = await Task.WhenAll(docs.Select(doc => MapToEntity(doc, _mapper)));

            return (entities.ToList() ?? new List<MessageEntity>()).OrderBy(m => m.CreatedAt).ToList();
        }

        public async Task AddMessagePairsAsync(List<MessageEntity> messagePairs)
        {
            await InsertListAsync(messagePairs);
        }

        public async Task<List<MessageEntity>> FindUnscoredAsync(int limit)
        {
            var sort        = Builders<MessageDocument>.Sort.Descending(x => x.CreatedAt);
            var docList     = await DB.Find<MessageDocument>()
                            .Match(x => x.Pair.WarningScore == -1 || x.Pair.WarningScore == null)
                            .Sort(d => d.Descending(x => x.CreatedAt))         // 從舊到新
                            .Limit(limit)
                            .ExecuteAsync();

            return _mapper.Map<List<MessageEntity>>(docList);
        }

        public async Task<int> UpdateWarningScoresAsync(List<WarningScoreUpdate> updates)
        {
            if (updates == null || updates.Count == 0) return 0;

            var now         = DateTime.UtcNow;
            var modified    = 0;

            foreach (var u in updates)
            {
                await DB.Update<MessageDocument>()
                    .MatchID(u.MessageId)
                    .Modify(m => m.Pair.WarningScore, u.WarningScore)
                    .Modify(m => m.UpdatedAt, now)                      
                    .ExecuteAsync();

                modified++;
            }

            return modified;
        }
    }
}
