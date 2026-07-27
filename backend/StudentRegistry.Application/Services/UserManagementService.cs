using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    // Users Management (Admin panel). Passwords are always hashed via the same IPasswordHasher<User>
    // (PBKDF2, salted) used for login — a raw password only ever exists in memory for the duration of
    // this request, never persisted or logged, and never read back out (UserResponseDto has no
    // password field at all). The seeded root admin ("Mohamed", User.IsProtected) can never be
    // edited, have its password changed, or be deleted through this service, by anyone — including
    // itself — no matter who is calling.
    public class UserManagementService : IUserManagementService
    {
        private const int MinPasswordLength = 6;

        private static readonly HashSet<string> ValidRoles = new()
        {
            AuthConstants.RoleViewer, AuthConstants.RoleEditor, AuthConstants.RoleAdmin
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserManagementService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher<User> passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto> CreateAsync(UserCreateDto createDto)
        {
            var username = (createDto.Username ?? string.Empty).Trim();
            ValidateUsername(username);
            ValidateRole(createDto.Role);
            ValidatePassword(createDto.Password);

            if (await _unitOfWork.Users.UsernameExistsAsync(username))
            {
                throw new ArgumentException("اسم المستخدم مستخدم بالفعل.");
            }

            var user = new User
            {
                Username = username,
                Role = createDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsProtected = false
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, createDto.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> UpdateAsync(int id, UserUpdateDto updateDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("المستخدم غير موجود.");

            if (user.IsProtected)
            {
                throw new InvalidOperationException("لا يمكن تعديل بيانات هذا المستخدم — حساب محمي.");
            }

            var username = (updateDto.Username ?? string.Empty).Trim();
            ValidateUsername(username);
            ValidateRole(updateDto.Role);

            if (await _unitOfWork.Users.UsernameExistsAsync(username, excludeUserId: id))
            {
                throw new ArgumentException("اسم المستخدم مستخدم بالفعل.");
            }

            user.Username = username;
            user.Role = updateDto.Role;
            user.IsActive = updateDto.IsActive;

            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task DeleteAsync(int id, string currentUsername)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("المستخدم غير موجود.");

            if (user.IsProtected)
            {
                throw new InvalidOperationException("لا يمكن حذف هذا المستخدم — حساب محمي.");
            }

            if (string.Equals(user.Username, currentUsername, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("لا يمكنك حذف حسابك الحالي.");
            }

            _unitOfWork.Users.Delete(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("المستخدم غير موجود.");

            if (user.IsProtected)
            {
                throw new InvalidOperationException("لا يمكن تغيير كلمة مرور هذا المستخدم — حساب محمي.");
            }

            ValidatePassword(changePasswordDto.NewPassword);

            user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordDto.NewPassword);
            await _unitOfWork.CompleteAsync();
        }

        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("اسم المستخدم مطلوب.");
            }
            if (username.Length > 50)
            {
                throw new ArgumentException("اسم المستخدم طويل جداً (الحد الأقصى 50 حرفاً).");
            }
        }

        private static void ValidateRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role) || !ValidRoles.Contains(role))
            {
                throw new ArgumentException("الصلاحية المحددة غير صالحة.");
            }
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            {
                throw new ArgumentException($"كلمة المرور يجب ألا تقل عن {MinPasswordLength} أحرف.");
            }
        }
    }
}
