using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = AuthConstants.RoleAdmin)]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? certification)
        {
            var result = await _dashboardService.GetStatsAsync(startDate, endDate, certification);
            return Ok(new { status = "success", data = result });
        }
    }
}
