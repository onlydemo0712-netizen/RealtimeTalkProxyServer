using Common.DTO.ChatRole;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;
using AetherCore.Controller;

namespace OpenAIProxyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatRoleController : GenericController<ChatRoleRequest, ChatRoleResponse, IChatRoleService>
    {
        public ChatRoleController(IChatRoleService service)
            : base(service)
        {

        }
    }
}
