using AutoMapper;
using Common.DTO.ChatRole;

namespace Common.DTO.ChatRole
{
    public class ChatRoleProfile : Profile
    {
        public ChatRoleProfile()
        {
            CreateMap<ChatRoleRequest, ChatRoleEntity>();
            CreateMap<ChatRoleEntity, ChatRoleResponse>();
            CreateMap<ChatRoleEntity, ChatRoleDocument>();
            CreateMap<ChatRoleDocument, ChatRoleEntity>();
        }
    }

}
