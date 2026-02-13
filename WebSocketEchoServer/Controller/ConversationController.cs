using Common.DTO.Auth;
using Common.DTO.Conversation;
using Common.DTO.Message;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;
using AetherCore.Controller;

namespace OpenAIProxyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationController : GenericController<ConversationRequest, ConversationResponse, IConversationService>
    {
        public ConversationController(IConversationService service)
            : base(service)
        {

        }

        [HttpPost("GetDetail")]
        public async Task<IActionResult> GetDetail([FromBody] GetDetailRequest request)
        {
            var bResult = await _service.GetDetail(request);

            return Ok(bResult);
        }
    }
}
