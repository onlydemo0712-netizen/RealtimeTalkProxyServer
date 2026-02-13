using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.Auth;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class AuthDataAccess : MongoEntityDataAccess<AuthEntity, AuthDocument>, IAuthDataAccess
    {
        public AuthDataAccess(IMapper mapper)
            : base(mapper)
        {
            EnsureIndexCreated("Account");
            AddNoUpdateKey("PasswordHash");
        }
    }
}
