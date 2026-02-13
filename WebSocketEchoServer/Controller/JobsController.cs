using Common.DTO.ChatRole;
using Common.DTO.Jobs;
using Common.Setting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Service.Interface;
using AetherCore.Controller;

namespace ElderAIServer.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : GenericController<JobsRequest, JobsResponse, IJobsService>
    {        
        private readonly JobsSettings _jobsSettings;

        public JobsController(IJobsService service, IOptions<JobsSettings> opt)
            : base(service)
        {
            _jobsSettings = opt.Value;
        }

        [HttpGet("DailyCheck")]
        public async Task<IActionResult> DailyCheck()
        {            
            var expected = _jobsSettings.InternalKey;
            var provided = Request.Headers["X-Internal-Key"].ToString();

            if (string.IsNullOrWhiteSpace(expected) || provided != expected)
                return Unauthorized("Invalid internal key.");

            var result = await _service.DailyCheck();

            return Ok(result);
        }
    }
}
