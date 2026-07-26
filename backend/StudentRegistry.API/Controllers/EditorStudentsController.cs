using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    // Read endpoints for the Student Records Editor page — deliberately separate from
    // StudentsController (which stays Viewer-only and untouched) rather than broadening its
    // [Authorize] roles, so the read-only Student Records Review page's API surface never changes.
    [ApiController]
    [Route("api/editor/students")]
    [Authorize(Roles = AuthConstants.RoleEditor)]
    public class EditorStudentsController : ControllerBase
    {
        private readonly IEditorStudentService _editorStudentService;

        public EditorStudentsController(IEditorStudentService editorStudentService)
        {
            _editorStudentService = editorStudentService;
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
    }
}
