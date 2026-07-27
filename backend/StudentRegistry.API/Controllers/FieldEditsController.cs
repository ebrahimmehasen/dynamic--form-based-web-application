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
    [Route("api/editor/fieldedits")]
    [Authorize(Roles = AuthConstants.RoleEditor)]
    public class FieldEditsController : ControllerBase
    {
        private readonly IFieldEditService _fieldEditService;

        public FieldEditsController(IFieldEditService fieldEditService)
        {
            _fieldEditService = fieldEditService;
        }

        // Handles a plain inline edit, "Approve as Edit", and "Write own edit" — all three are the
        // same shape, differing only in Source/TriggeringCommentId (see FieldEditCreateDto).
        [HttpPost]
        public async Task<IActionResult> ApplyEdit([FromBody] FieldEditCreateDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.EntityGroup) || string.IsNullOrWhiteSpace(createDto.PropertyName))
            {
                return BadRequest(new { status = "error", message = "بيانات الحقل غير مكتملة." });
            }

            try
            {
                var editorUsername = User.Identity?.Name ?? "Editor";
                var result = await _fieldEditService.ApplyEditAsync(createDto, editorUsername);
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

        [HttpGet("student/{studentId:int}")]
        public async Task<IActionResult> GetForStudent(int studentId)
        {
            var result = await _fieldEditService.GetForStudentAsync(studentId);
            return Ok(new { status = "success", data = result });
        }
    }
}
