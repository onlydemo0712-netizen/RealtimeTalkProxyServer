using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.DTO.Auth;
using Service.Interface;
using AetherCore.Controller;

namespace OpenAIProxyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : GenericController<IdentityRequest, LoginResponse, IAuthService>
    {
        public AuthController(IAuthService service)
            : base(service)
        {

        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] IdentityRequest request)
        {
            var bResult = await _service.Login(request);

            return Ok(bResult);
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var bResult = await _service.ChangePassword(request);

            return Ok(bResult);
        }
    }
}
