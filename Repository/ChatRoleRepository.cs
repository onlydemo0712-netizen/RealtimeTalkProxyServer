using AetherCore.Repository;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.Caches;
using Common.DTO.ChatRole;
using DataAccess.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Repository.Interface;

namespace Repository
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class ChatRoleRepository : GenericRepository<ChatRoleEntity, IChatRoleDataAccess>, IChatRoleRepository
    {
        public ChatRoleRepository(IChatRoleDataAccess dataAccess, IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings) 
            : base(dataAccess, memoryCache, cacheSettings)
        {

        }
    }
}
