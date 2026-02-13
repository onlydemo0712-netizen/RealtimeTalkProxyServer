using AetherCore.DataAccess;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.ChatRole;
using DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class ChatRoleDataAccess : MongoEntityDataAccess<ChatRoleEntity, ChatRoleDocument>, IChatRoleDataAccess
    {
        public ChatRoleDataAccess(IMapper mapper)
            : base(mapper)
        {
            EnsureIndexCreated("RoleId");
        }
    }
}
