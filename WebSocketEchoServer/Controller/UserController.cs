using Common.DTO.Auth;
using Common.DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;
using System.Security.Claims;
using AetherCore.Controller;

namespace OpenAIProxyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : GenericController<UserRequest, UserResponse, IUserService>
    {
        private readonly IAuthService _authService;

        public UserController(IUserService service, IAuthService authService)
            : base(service)
        {
            _authService = authService;
        }

        [HttpPost("CreateIdentity")]
        public async Task<IActionResult> CreateIdentity([FromBody] IdentityRequest request)
        {
            // 先創帳號 失敗會error
            bool success = await _service.CreateIdentity(request);
            if (success)
            {
                await _authService.CreateIdentity(request);

                // 失敗就刪除原本生成的帳號
                if (!success)
                    await _service.DeleteAsync(request.Account);
            }
            
            return Ok(success);
        }

        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId  = User.FindFirstValue(ClaimTypes.Name);      
            var bResult = await _service.GetAsync(userId);

            return Ok(bResult);
        }

        [HttpPost("TrackInfo")]
        public async Task<IActionResult> TrackInfo([FromBody] ActivityTrackRequest request)
        {
            var userId  = User.FindFirstValue(ClaimTypes.Name);
            var bResult = await _service.TrackInfo(userId, request);

            return Ok(bResult);
        }

        [HttpPost("QuotaChange")]
        public async Task<IActionResult> QuotaLimitChange([FromBody] QuotaLimitChangeRequest request)
        {
            var bResult = await _service.QuotaLimitChange(request);

            return Ok(bResult);
        }
    }
}
