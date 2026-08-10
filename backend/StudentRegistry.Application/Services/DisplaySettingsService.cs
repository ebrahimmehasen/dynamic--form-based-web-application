using Microsoft.AspNetCore.Identity;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    // Backs the admin-only "إعدادات العرض" tab — per-certification toggles for whether the public
    // registration success screen shows the computed final score. Restricted (in the controller) to
    // the protected root admin ("Mohamed"); this service additionally enforces the re-entered
    // account-password confirmation, verified via the same hasher used at login (AuthService).
    public class DisplaySettingsService : IDisplaySettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;

        public DisplaySettingsService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<CertificationDisplaySettingDto>> GetAllAsync()
        {
            var existing = (await _unitOfWork.CertificationDisplaySettings.GetAllAsync())
                .ToDictionary(s => s.CertificationKey);

            return CertificationCatalog.All.Select(c =>
            {
                existing.TryGetValue(c.Key, out var setting);
                return new CertificationDisplaySettingDto
                {
                    CertificationKey = c.Key,
                    Label = c.Label,
                    IsResultVisible = setting?.IsResultVisible ?? true,
                    UpdatedAt = setting?.UpdatedAt,
                    UpdatedByUsername = setting?.UpdatedByUsername
                };
            }).ToList();
        }

        public async Task<Dictionary<string, bool>> GetVisibilityMapAsync()
        {
            var existing = (await _unitOfWork.CertificationDisplaySettings.GetAllAsync())
                .ToDictionary(s => s.CertificationKey, s => s.IsResultVisible);

            return CertificationCatalog.All.ToDictionary(
                c => c.Key,
                c => existing.TryGetValue(c.Key, out var isVisible) ? isVisible : true);
        }

        public async Task ToggleAsync(string certificationKey, bool isVisible, string password, User currentUser)
        {
            if (!CertificationCatalog.All.Any(c => c.Key == certificationKey))
                throw new ArgumentException("نوع شهادة غير معروف.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("يجب إدخال كلمة مرور الحساب لتأكيد التعديل.");

            var verification = _passwordHasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash, password);
            if (verification == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("كلمة المرور غير صحيحة.");

            var setting = await _unitOfWork.CertificationDisplaySettings.GetByKeyAsync(certificationKey);
            if (setting == null)
            {
                setting = new CertificationDisplaySetting
                {
                    CertificationKey = certificationKey,
                    IsResultVisible = isVisible,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUsername = currentUser.Username
                };
                await _unitOfWork.CertificationDisplaySettings.AddAsync(setting);
            }
            else
            {
                setting.IsResultVisible = isVisible;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedByUsername = currentUser.Username;
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
