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
        private readonly IEligibilityExportService _eligibilityExportService;

        public AdminDashboardController(IAdminDashboardService dashboardService, IEligibilityExportService eligibilityExportService)
        {
            _dashboardService = dashboardService;
            _eligibilityExportService = eligibilityExportService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? certification)
        {
            var result = await _dashboardService.GetStatsAsync(startDate, endDate, certification);
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("export/eligible")]
        public async Task<IActionResult> ExportEligible([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? certification)
        {
            var fileBytes = await _eligibilityExportService.ExportByEligibilityAsync("Eligible", startDate, endDate, certification);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الطلاب_المستوفين.xlsx");
        }

        [HttpGet("export/not-eligible")]
        public async Task<IActionResult> ExportNotEligible([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? certification)
        {
            var fileBytes = await _eligibilityExportService.ExportByEligibilityAsync("NotEligible", startDate, endDate, certification);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الطلاب_غير_المستوفين.xlsx");
        }

        [HttpGet("export/all")]
        public async Task<IActionResult> ExportAll([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? certification)
        {
            var fileBytes = await _eligibilityExportService.ExportAllAsync(startDate, endDate, certification);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "كل_الطلاب.xlsx");
        }
    }
}
