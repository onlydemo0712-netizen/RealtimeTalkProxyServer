using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.AdminAuth;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class AdminAuthDataAccess : MongoEntityDataAccess<AdminAuthEntity, AdminAuthDocument>, IAdminAuthDataAccess
    {
        public AdminAuthDataAccess(IMapper mapper)
            : base(mapper)
        {
            EnsureIndexCreated("Account");
            AddNoUpdateKey("PasswordHash");
        }
    }
}
