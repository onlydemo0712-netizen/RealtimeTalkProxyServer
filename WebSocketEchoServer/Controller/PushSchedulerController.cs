using Common.DTO.ChatRole;
using Common.DTO.Conversation;
using Common.DTO.Jobs;
using Common.DTO.PushScheduler;
using Common.Setting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service;
using Service.Interface;
using AetherCore.Controller;

namespace ElderAIServer.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class PushSchedulerController : GenericController<PushSchedulerRequest, PushSchedulerResponse, IPushSchedulerService>
    {
        private readonly JobsSettings _opt;

        public PushSchedulerController(IPushSchedulerService service, IOptions<JobsSettings> opt)
            : base(service)
        {
            _opt = opt.Value;
        }

        [HttpPost("RunPushScheduler")]
        public async Task<IActionResult> RunPushScheduler()
        {
            var expected = _opt.InternalKey;
            var provided = Request.Headers["X-Internal-Key"].ToString();

            if (string.IsNullOrWhiteSpace(expected) || provided != expected)
                return Unauthorized("Invalid internal key.");

            var result = await _service.RunPushScheduler();

            return Ok();
        }

        [HttpPost("PushMsgAllImmediately")]
        public async Task<IActionResult> PushMsgAllImmediately([FromBody] PushMsgRequest request)
        {
            var result = await _service.PushMsgAllImmediately(request);

            return Ok();
        }
    }
}
