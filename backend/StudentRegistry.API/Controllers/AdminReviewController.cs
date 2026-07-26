using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    // Admin oversight of Editor activity: global (cross-student) field-edit/comment/delete-request
    // logs, and the only place delete requests can be approved (actually deletes the student row,
    // cascading through the existing cert-table FKs) or rejected.
    [ApiController]
    [Route("api/admin/review")]
    [Authorize(Roles = AuthConstants.RoleAdmin)]
    public class AdminReviewController : ControllerBase
    {
        private readonly IAdminReviewService _adminReviewService;

        public AdminReviewController(IAdminReviewService adminReviewService)
        {
            _adminReviewService = adminReviewService;
        }

        [HttpGet("fieldedits")]
        public async Task<IActionResult> GetFieldEdits([FromQuery] bool fromCommentOnly = false)
        {
            var result = await _adminReviewService.GetFieldEditsAsync(fromCommentOnly);
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("fieldcomments")]
        public async Task<IActionResult> GetFieldComments()
        {
            var result = await _adminReviewService.GetFieldCommentsAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("deleterequests")]
        public async Task<IActionResult> GetDeleteRequests([FromQuery] string? status)
        {
            var result = await _adminReviewService.GetDeleteRequestsAsync(status);
            return Ok(new { status = "success", data = result });
        }

        [HttpPost("deleterequests/{id:int}/approve")]
        public async Task<IActionResult> ApproveDeleteRequest(int id)
        {
            var result = await _adminReviewService.ApproveDeleteAsync(id);
            if (result == null)
            {
                return NotFound(new { status = "error", message = "طلب الحذف غير موجود أو تم البت فيه مسبقاً." });
            }
            return Ok(new { status = "success", data = result });
        }

        [HttpPost("deleterequests/{id:int}/reject")]
        public async Task<IActionResult> RejectDeleteRequest(int id)
        {
            var result = await _adminReviewService.RejectDeleteAsync(id);
            if (result == null)
            {
                return NotFound(new { status = "error", message = "طلب الحذف غير موجود أو تم البت فيه مسبقاً." });
            }
            return Ok(new { status = "success", data = result });
        }
    }
}
