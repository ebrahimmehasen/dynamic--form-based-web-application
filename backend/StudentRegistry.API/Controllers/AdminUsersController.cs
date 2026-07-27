using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    // Admin-only user management (list/create/edit/delete/change-password). Only admins may reach
    // this at all; the protected-root-admin and self-delete guards live in UserManagementService so
    // they hold regardless of caller.
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = AuthConstants.RoleAdmin)]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public AdminUsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userManagementService.GetAllAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto createDto)
        {
            try
            {
                var result = await _userManagementService.CreateAsync(createDto);
                return Ok(new { status = "success", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto updateDto)
        {
            try
            {
                var result = await _userManagementService.UpdateAsync(id, updateDto);
                return Ok(new { status = "success", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { status = "error", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUsername = User.Identity?.Name ?? string.Empty;
            try
            {
                await _userManagementService.DeleteAsync(id, currentUsername);
                return Ok(new { status = "success" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("{id:int}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                await _userManagementService.ChangePasswordAsync(id, changePasswordDto);
                return Ok(new { status = "success" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { status = "error", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }
    }
}
