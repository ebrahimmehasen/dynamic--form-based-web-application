using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Placeholder endpoint — real credential validation, session/token issuance, and redirect
        // logic will replace AuthService's implementation later. The request/response shape here
        // (LoginRequestDto/LoginResultDto) is the stable contract the frontend already speaks, so
        // wiring in real auth later needs no frontend changes.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            // 501 Not Implemented — accurately reflects that this endpoint does not yet perform
            // authentication, rather than returning a misleading 200/401.
            return StatusCode(501, result);
        }
    }
}
