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
    [ApiController]
    [Route("api/editor/fieldcomments")]
    [Authorize(Roles = AuthConstants.RoleEditor)]
    public class FieldCommentsController : ControllerBase
    {
        private readonly IFieldCommentService _fieldCommentService;

        public FieldCommentsController(IFieldCommentService fieldCommentService)
        {
            _fieldCommentService = fieldCommentService;
        }

        [HttpGet("student/{studentId:int}")]
        public async Task<IActionResult> GetForStudent(int studentId)
        {
            var result = await _fieldCommentService.GetForStudentAsync(studentId);
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("unreviewed")]
        public async Task<IActionResult> GetUnreviewed()
        {
            var result = await _fieldCommentService.GetUnreviewedAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("resolved")]
        public async Task<IActionResult> GetResolved()
        {
            var result = await _fieldCommentService.GetResolvedAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("unreviewed/count")]
        public async Task<IActionResult> GetUnreviewedCount()
        {
            var result = await _fieldCommentService.GetUnreviewedCountAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] FieldCommentCreateDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.EntityGroup) || string.IsNullOrWhiteSpace(createDto.PropertyName)
                || string.IsNullOrWhiteSpace(createDto.CommentText))
            {
                return BadRequest(new { status = "error", message = "بيانات التعليق غير مكتملة." });
            }

            try
            {
                var result = await _fieldCommentService.AddCommentAsync(createDto);
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
