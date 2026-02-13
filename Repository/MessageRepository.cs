using AetherCore.Repository;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.Caches;
using Common.DTO.Message;
using DataAccess.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Repository.Interface;

namespace Repository
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class MessageRepository : GenericRepository<MessageEntity, IMessageDataAccess>, IMessageRepository
    {
        public MessageRepository(IMessageDataAccess dataAccess, IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings) 
            : base(dataAccess, memoryCache, cacheSettings)
        {

        }

        public async Task<List<MessagePair>> GetDetail(string conversationId)
        {
            List<MessageEntity> entityList  = await _dataAccess.GetDetail(conversationId);
            List<MessagePair> result        = entityList.Select(e => e.Pair).ToList();

            return result;
        }

        public async Task AddMessagePairsAsync(string conversatonId, List<MessagePair> messagePairs)
        {
            List<MessageEntity> entitys = messagePairs.Select(p => new MessageEntity() 
            {
                ConversationId  = conversatonId,
                Pair            = p
            }).ToList();

            await _dataAccess.AddMessagePairsAsync(entitys);
        }

        public async Task<int> UpdateWarningScoresAsync(List<WarningScoreUpdate> updates)
        {
            return await _dataAccess.UpdateWarningScoresAsync(updates);
        }

        public async Task<List<MessageEntity>> FindUnscoredAsync(int limit)
        {
            return await _dataAccess.FindUnscoredAsync(limit);
        }
    }
}
