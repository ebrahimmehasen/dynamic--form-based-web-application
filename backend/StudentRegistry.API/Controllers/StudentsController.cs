using FluentValidation;
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
    // Student data (name, national ID, guardian info, grades...) is sensitive — every endpoint
    // here requires the Viewer role except the public registration form itself, which stays
    // anonymous on purpose (it's the student-facing submission flow, not an internal review tool).
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AuthConstants.RoleViewer)]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IValidator<StudentCreateDto> _validator;
        private readonly IStudentExcelExportService _excelExportService;
        private readonly IPendingReviewService _pendingReviewService;

        public StudentsController(
            IStudentService studentService,
            IValidator<StudentCreateDto> validator,
            IStudentExcelExportService excelExportService,
            IPendingReviewService pendingReviewService)
        {
            _studentService = studentService;
            _validator = validator;
            _excelExportService = excelExportService;
            _pendingReviewService = pendingReviewService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterStudent([FromBody] StudentCreateDto createDto)
        {
            // Validate creation request
            var validationResult = await _validator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                // Capture first error message to match original alert style
                var firstError = validationResult.Errors[0].ErrorMessage;
                return BadRequest(new { status = "error", message = firstError });
            }

            try
            {
                var result = await _studentService.RegisterStudentAsync(createDto);
                return Ok(new { status = "success", message = "تم حفظ استمارة الطالب بنجاح.", file_path = result.PhotoPath, data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع أثناء المعالجة على الخادم.", details = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var result = await _studentService.GetStudentByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { status = "error", message = "الطالب غير موجود." });
            }
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("nationalId/{nationalId}")]
        public async Task<IActionResult> GetStudentByNationalId(string nationalId)
        {
            if (string.IsNullOrEmpty(nationalId))
            {
                return BadRequest(new { status = "error", message = "الرقم القومي مطلوب." });
            }

            var result = await _studentService.GetStudentByNationalIdAsync(nationalId);
            if (result == null)
            {
                return NotFound(new { status = "error", message = "لا توجد استمارة مسجلة بهذا الرقم القومي." });
            }
            return Ok(new { status = "success", data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var result = await _studentService.GetAllStudentsAsync();
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string? q)
        {
            var result = await _studentService.SearchStudentsAsync(q);
            return Ok(new { status = "success", data = result });
        }

        [HttpGet("{id:int}/export")]
        public async Task<IActionResult> ExportStudent(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound(new { status = "error", message = "الطالب غير موجود." });
            }

            var fileBytes = await _excelExportService.ExportStudentAsync(id);
            string fileName = $"{student.NationalId}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Public, anonymous download for the student who just registered — gated by requiring the
        // caller to also supply the matching national ID (known only to the registrant/owner),
        // not just the numeric id, since this bypasses the Viewer-role auth on the rest of this controller.
        [HttpGet("{id:int}/export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportStudentPdf(int id, [FromQuery] string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
            {
                return BadRequest(new { status = "error", message = "الرقم القومي مطلوب." });
            }

            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null || !string.Equals(student.NationalId, nationalId, StringComparison.Ordinal))
            {
                return NotFound(new { status = "error", message = "الطالب غير موجود." });
            }

            var fileBytes = await _excelExportService.ExportStudentPdfAsync(id);
            string fileName = $"{student.NationalId}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }

        // Viewer-only: flags a student "قيد المراجعة" so both the Viewer's and Editor's tables
        // highlight the row until an Editor resolves it (see EditorStudentsController.ResolvePendingReview).
        [HttpPost("{id:int}/pending-review")]
        public async Task<IActionResult> MarkPendingReview(int id)
        {
            try
            {
                var flaggedBy = User.Identity?.Name ?? "User";
                var result = await _pendingReviewService.FlagAsync(id, flaggedBy);
                return Ok(new { status = "success", data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }
    }
}
