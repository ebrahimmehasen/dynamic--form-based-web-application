using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Application.Interfaces
{
    public interface IDisplaySettingsService
    {
        Task<List<CertificationDisplaySettingDto>> GetAllAsync();

        // Public map (certificationKey -> isResultVisible) — for ConfigController to fold into the
        // public /api/config/subjects response the registration page loads.
        Task<Dictionary<string, bool>> GetVisibilityMapAsync();

        Task ToggleAsync(string certificationKey, bool isVisible, string password, User currentUser);
    }
}
