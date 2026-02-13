using AetherCore.Service;
using AetherCore.Utility.Attributes;
using AutoMapper;
using Common.DTO.ChatRole;
using Microsoft.Extensions.DependencyInjection;
using Repository.Interface;
using Service.Interface;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class ChatRoleService : GenericService<ChatRoleEntity, ChatRoleRequest, ChatRoleResponse, IChatRoleRepository>, IChatRoleService
    {
        public ChatRoleService(IChatRoleRepository repo, IMapper mapper) 
            : base(repo, mapper)
        {
        }
    }
}
