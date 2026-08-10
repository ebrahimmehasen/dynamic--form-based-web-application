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
    // No class-level [Authorize] — each action sets its own roles, since Viewer may create/read its
    // own comments but must never see the cross-student inbox (unreviewed/resolved/count) or dismiss.
    [ApiController]
    [Route("api/editor/fieldcomments")]
    public class FieldCommentsController : ControllerBase
    {
        private readonly IFieldCommentService _fieldCommentService;

        public FieldCommentsController(IFieldCommentService fieldCommentService)
        {
            _fieldCommentService = fieldCommentService;
        }

        // Viewer needs this to show its own comment history/highlighting on the Student Records
        // Review page (§ review-page.js) — same read access Editor/Admin already have.
        [HttpGet("student/{studentId:int}")]
        [Authorize(Roles = AuthConstants.RoleEditor + "," + AuthConstants.RoleAdmin + "," + AuthConstants.RoleViewer)]
        public async Task<IActionResult> GetForStudent(int studentId)
        {
            var result = await _fieldCommentService.GetForStudentAsync(studentId);
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("unreviewed")]
        [Authorize(Roles = AuthConstants.RoleEditor + "," + AuthConstants.RoleAdmin)]
        public async Task<IActionResult> GetUnreviewed()
        {
            var result = await _fieldCommentService.GetUnreviewedAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("resolved")]
        [Authorize(Roles = AuthConstants.RoleEditor + "," + AuthConstants.RoleAdmin)]
        public async Task<IActionResult> GetResolved()
        {
            var result = await _fieldCommentService.GetResolvedAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("unreviewed/count")]
        [Authorize(Roles = AuthConstants.RoleEditor + "," + AuthConstants.RoleAdmin)]
        public async Task<IActionResult> GetUnreviewedCount()
        {
            var result = await _fieldCommentService.GetUnreviewedCountAsync();
            return Ok(new { status = "success", data = result });
        }

        // Viewer is now allowed here too — this is the actual fix: Viewer's review-page.js posts
        // its "review notes" into the same FieldComments table/workflow Editor already reviews,
        // instead of the old, dead-end ReviewNotes table Editor never looked at.
        [HttpPost]
        [Authorize(Roles = AuthConstants.RoleEditor + "," + AuthConstants.RoleAdmin + "," + AuthConstants.RoleViewer)]
        public async Task<IActionResult> AddComment([FromBody] FieldCommentCreateDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.EntityGroup) || string.IsNullOrWhiteSpace(createDto.PropertyName)
                || string.IsNullOrWhiteSpace(createDto.CommentText))
            {
                return BadRequest(new { status = "error", message = "بيانات التعليق غير مكتملة." });
            }

            try
            {
                var authorUsername = User.Identity?.Name ?? "Editor";
                var result = await _fieldCommentService.AddCommentAsync(createDto, authorUsername);
                return Ok(new { status = "success", data = result });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { status = "error", message = "الطالب غير موجود." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("{id:int}/dismiss")]
        [Authorize(Roles = AuthConstants.RoleEditor + "," + AuthConstants.RoleAdmin)]
        public async Task<IActionResult> Dismiss(int id)
        {
            var result = await _fieldCommentService.DismissAsync(id);
            if (result == null)
            {
                return NotFound(new { status = "error", message = "التعليق غير موجود." });
            }
            return Ok(new { status = "success", data = result });
        }
    }
}
