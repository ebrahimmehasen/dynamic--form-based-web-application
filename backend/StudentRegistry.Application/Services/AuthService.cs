using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    // Real credential verification against dbo.Users. Never compares plain-text passwords — the
    // stored PasswordHash is produced by PasswordHasher<User> (PBKDF2, salted, versioned), and the
    // same hasher verifies the submitted password here. Username lookup is a parameterized EF Core
    // query (StudentRegistry.Repository.UserRepository), never raw SQL/string concatenation.
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
        {
            const string genericFailureMessage = "اسم المستخدم أو كلمة المرور غير صحيحة.";

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("محاولة تسجيل دخول برقم مستخدم أو كلمة مرور فارغة.");
                return new LoginResultDto { Success = false, Message = genericFailureMessage };
            }

            var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username.Trim());

            // Same generic message whether the username doesn't exist, the account is inactive, or
            // the password is wrong — never reveal which one, so a login attempt can't be used to
            // enumerate valid usernames. The log — unlike the response — CAN distinguish the reason,
            // since it's only ever read by staff, never returned to the client.
            if (user == null)
            {
                _logger.LogWarning("محاولة تسجيل دخول فاشلة: اسم المستخدم {Username} غير موجود.", request.Username);
                return new LoginResultDto { Success = false, Message = genericFailureMessage };
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("محاولة تسجيل دخول فاشلة: الحساب {Username} غير مفعّل.", user.Username);
                return new LoginResultDto { Success = false, Message = genericFailureMessage };
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("محاولة تسجيل دخول فاشلة: كلمة مرور خاطئة للمستخدم {Username}.", user.Username);
                return new LoginResultDto { Success = false, Message = genericFailureMessage };
            }

            _logger.LogInformation("تسجيل دخول ناجح للمستخدم {Username} بدور {Role}.", user.Username, user.Role);

            return new LoginResultDto
            {
                Success = true,
                Message = "تم تسجيل الدخول بنجاح.",
                Username = user.Username,
                Role = user.Role,
                RedirectUrl = AuthConstants.GetRedirectUrl(user.Role)
            };
        }
    }
}
