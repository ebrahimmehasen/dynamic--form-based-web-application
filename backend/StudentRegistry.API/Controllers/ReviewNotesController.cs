using Microsoft.AspNetCore.Mvc;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewNotesController : ControllerBase
    {
        private readonly IReviewNoteService _reviewNoteService;

        public ReviewNotesController(IReviewNoteService reviewNoteService)
        {
            _reviewNoteService = reviewNoteService;
        }

        [HttpGet("student/{studentId:int}")]
        public async Task<IActionResult> GetNotesForStudent(int studentId)
        {
            var result = await _reviewNoteService.GetNotesForStudentAsync(studentId);
            return Ok(new { status = "success", data = result });
        }

        [HttpPost]
        public async Task<IActionResult> AddNote([FromBody] ReviewNoteCreateDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.FieldName) || string.IsNullOrWhiteSpace(createDto.ReviewerNote))
            {
                return BadRequest(new { status = "error", message = "اسم الحقل ونص الملاحظة مطلوبان." });
            }

            var result = await _reviewNoteService.AddNoteAsync(createDto);
            if (result == null)
            {
                return NotFound(new { status = "error", message = "الطالب غير موجود." });
            }

            return Ok(new { status = "success", data = result });
        }
    }
}
