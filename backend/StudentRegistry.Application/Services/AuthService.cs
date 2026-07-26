using Microsoft.AspNetCore.Identity;
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

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
        {
            const string genericFailureMessage = "اسم المستخدم أو كلمة المرور غير صحيحة.";

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResultDto { Success = false, Message = genericFailureMessage };
            }

            var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username.Trim());

            // Same generic message whether the username doesn't exist, the account is inactive, or
            // the password is wrong — never reveal which one, so a login attempt can't be used to
            // enumerate valid usernames.
            if (user == null || !user.IsActive)
            {
                return new LoginResultDto { Success = false, Message = genericFailureMessage };
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return new LoginResultDto { Success = false, Message = genericFailureMessage };
            }

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
