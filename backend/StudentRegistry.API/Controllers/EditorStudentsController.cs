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
    // Read endpoints for the Student Records Editor page — deliberately separate from
    // StudentsController (which stays Viewer-only and untouched) rather than broadening its
    // [Authorize] roles, so the read-only Student Records Review page's API surface never changes.
    [ApiController]
    [Route("api/editor/students")]
    [Authorize(Roles = AuthConstants.RoleEditor + "," + AuthConstants.RoleAdmin)]
    public class EditorStudentsController : ControllerBase
    {
        private readonly IEditorStudentService _editorStudentService;
        private readonly IPendingReviewService _pendingReviewService;

        public EditorStudentsController(IEditorStudentService editorStudentService, IPendingReviewService pendingReviewService)
        {
            _editorStudentService = editorStudentService;
            _pendingReviewService = pendingReviewService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _editorStudentService.SearchAsync(q, page, pageSize);
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _editorStudentService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { status = "error", message = "الطالب غير موجود." });
            }
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("{id:int}/audit")]
        public async Task<IActionResult> GetAuditLog(int id)
        {
            var result = await _editorStudentService.GetAuditLogAsync(id);
            return Ok(new { status = "success", data = result });
        }

        // Re-runs the same per-certificate formulas used at registration against the student's
        // current (possibly Editor-edited) raw grade rows, and overwrites the totals accordingly.
        // Every changed total field is logged in the edits sheet under this editor's own username.
        [HttpPost("{id:int}/recalculate")]
        public async Task<IActionResult> Recalculate(int id)
        {
            var editorUsername = User.Identity?.Name ?? "Editor";
            try
            {
                var result = await _editorStudentService.RecalculateAsync(id, editorUsername);
                return Ok(new { status = "success", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // Editor-only: clears the "قيد المراجعة" flag a Viewer set on this student, once the Editor
        // has actually reviewed/fixed the data (see StudentsController.MarkPendingReview).
        [HttpPost("{id:int}/resolve-pending-review")]
        public async Task<IActionResult> ResolvePendingReview(int id)
        {
            try
            {
                var resolvedBy = User.Identity?.Name ?? "Editor";
                var result = await _pendingReviewService.ResolveAsync(id, resolvedBy);
                return Ok(new { status = "success", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
        }

        // Editor-only: confirms whether this student's submission is "مستوفي" (Eligible) or
        // "غير مستوفي" (NotEligible), logged in the edits sheet under this editor's own username.
        [HttpPost("{id:int}/eligibility")]
        public async Task<IActionResult> SetEligibility(int id, [FromBody] SetEligibilityDto dto)
        {
            try
            {
                var confirmedBy = User.Identity?.Name ?? "Editor";
                var result = await _editorStudentService.SetEligibilityAsync(id, dto.Status, dto.Note, confirmedBy);
                return Ok(new { status = "success", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }
    }
}
