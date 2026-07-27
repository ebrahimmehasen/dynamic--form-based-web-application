using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IFieldCommentService
    {
        // Throws KeyNotFoundException when the student doesn't exist.
        Task<FieldCommentResponseDto> AddCommentAsync(FieldCommentCreateDto createDto, string authorUsername);
        Task<IEnumerable<FieldCommentResponseDto>> GetForStudentAsync(int studentId);
        Task<IEnumerable<FieldCommentResponseDto>> GetUnreviewedAsync();
        Task<IEnumerable<FieldCommentResponseDto>> GetResolvedAsync();
        Task<int> GetUnreviewedCountAsync();
        Task<FieldCommentResponseDto?> DismissAsync(int id);
    }
}
