using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IReviewNoteService
    {
        Task<IEnumerable<ReviewNoteResponseDto>> GetNotesForStudentAsync(int studentId);

        // Returns null if the referenced student doesn't exist.
        Task<ReviewNoteResponseDto?> AddNoteAsync(ReviewNoteCreateDto createDto);
    }
}
