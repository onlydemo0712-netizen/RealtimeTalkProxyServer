using Common.DTO.Conversation;
using Common.DTO.KnowledgeGuide;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;
using System.Security.Claims;
using AetherCore.Controller;

namespace OpenAIProxyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KnowledgeGuideController : GenericController<KnowledgeGuideRequest, KnowledgeGuideResponse, IKnowledgeGuideService>
    {
        public KnowledgeGuideController(IKnowledgeGuideService service)
            : base(service)
        {

        }

        [HttpPost("ImgUpload")]
        public async Task<IActionResult> ImgUpload([FromBody] ImgUploadRequest request)
        {
            var bResult = await _service.ImgUpload(request);

            return Ok(bResult);
        }
    }
}
