using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using System;
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

        public StudentsController(IStudentService studentService, IValidator<StudentCreateDto> validator)
        {
            _studentService = studentService;
            _validator = validator;
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
    }
}
