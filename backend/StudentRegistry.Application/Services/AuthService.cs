using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    // Placeholder only — no credential verification, no database lookup, no session/token issuance
    // yet. Deliberately never returns Success = true, so the frontend can't be mistaken for a real
    // login. Replace the body of LoginAsync with real authentication logic; AuthController and the
    // Login page do not need to change when that happens.
    public class AuthService : IAuthService
    {
        public Task<LoginResultDto> LoginAsync(LoginRequestDto request)
        {
            var result = new LoginResultDto
            {
                Success = false,
                Message = "تسجيل الدخول غير مفعل بعد. سيتم تفعيله لاحقاً."
            };

            return Task.FromResult(result);
        }
    }
}
