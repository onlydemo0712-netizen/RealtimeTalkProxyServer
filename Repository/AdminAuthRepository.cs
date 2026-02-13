using AetherCore.DTO;
using AetherCore.Repository;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.Caches;
using Common.DTO.AdminAuth;
using DataAccess.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Repository.Interface;

namespace Repository
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class AdminAuthRepository : GenericRepository<AdminAuthEntity, IAdminAuthDataAccess>, IAdminAuthRepository
    {
        public AdminAuthRepository(IAdminAuthDataAccess dataAccess, IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings) 
            : base(dataAccess, memoryCache, cacheSettings)
        {

        }

        public async Task<bool> ChangePassword(string account, string newPasswordHash)
        {
            var definition = new FindUpdateDefinition<AdminAuthEntity>()
                .Where(_ => Builders<AdminAuthEntity>.Filter.Eq(x => x.Account, account))
                .UpdateDef(_ => Builders<AdminAuthEntity>.Update
                    .Set(x => x.PasswordHash, newPasswordHash)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow)
                );

            var updated = await _dataAccess.FindOneAndUpdateAsync(definition);
            return updated != null;
        }
    }
}
