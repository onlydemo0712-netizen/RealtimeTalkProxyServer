using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.User;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class UserDataAccess : MongoEntityDataAccess<UserEntity, UserDocument>, IUserDataAccess
    {
        public UserDataAccess(IMapper mapper)
            : base(mapper)
        {
            EnsureIndexCreated("UserId");
        }
    }
}
