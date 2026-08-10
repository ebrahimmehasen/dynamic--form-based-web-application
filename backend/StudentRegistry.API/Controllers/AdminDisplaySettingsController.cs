using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    // "إعدادات العرض" — per-certification toggles for whether the public success screen shows the
    // final score. Every action here (including read) is restricted to protected root admins
    // (User.IsProtected) — no other Admin-role account may view or change these, per the feature's
    // own spec. Any number of protected admins can exist (e.g. "Mohamed", "Ebrahim").
    [ApiController]
    [Route("api/admin/display-settings")]
    [Authorize(Roles = AuthConstants.RoleAdmin)]
    public class AdminDisplaySettingsController : ControllerBase
    {
        private readonly IDisplaySettingsService _displaySettingsService;
        private readonly IUnitOfWork _unitOfWork;

        public AdminDisplaySettingsController(IDisplaySettingsService displaySettingsService, IUnitOfWork unitOfWork)
        {
            _displaySettingsService = displaySettingsService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!await IsRootAdminAsync())
                return Forbid();

            var result = await _displaySettingsService.GetAllAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> Toggle(string key, [FromBody] ToggleDisplaySettingDto dto)
        {
            var currentUser = await _unitOfWork.Users.GetByUsernameAsync(User.Identity?.Name ?? string.Empty);
            if (currentUser == null || !currentUser.IsProtected)
                return Forbid();

            try
            {
                await _displaySettingsService.ToggleAsync(key, dto.IsVisible, dto.Password, currentUser);
                return Ok(new { status = "success" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { status = "error", message = ex.Message });
            }
        }

        private async Task<bool> IsRootAdminAsync()
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(User.Identity?.Name ?? string.Empty);
            return user != null && user.IsProtected;
        }
    }
}
