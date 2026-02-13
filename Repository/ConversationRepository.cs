using AetherCore.Repository;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.Caches;
using Common.DTO.Conversation;
using DataAccess.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Repository.Interface;

namespace Repository
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class ConversationRepository : GenericRepository<ConversationEntity, IConversationDataAccess>, IConversationRepository
    {
        public ConversationRepository(IConversationDataAccess dataAccess, IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings) 
            : base(dataAccess, memoryCache, cacheSettings)
        {

        }
    }
}
