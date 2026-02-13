using AetherCore.Repository;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.Caches;
using Common.DTO.KnowledgeGuide;
using DataAccess.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Repository.Interface;

namespace Repository
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class KnowledgeGuideRepository : GenericRepository<KnowledgeGuideEntity, IKnowledgeGuideDataAccess>, IKnowledgeGuideRepository
    {
        public KnowledgeGuideRepository(IKnowledgeGuideDataAccess dataAccess, IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings) 
            : base(dataAccess, memoryCache, cacheSettings)
        {

        }
    }
}
