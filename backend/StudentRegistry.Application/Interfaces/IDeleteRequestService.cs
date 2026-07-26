using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IDeleteRequestService
    {
        // Throws KeyNotFoundException when the student doesn't exist, InvalidOperationException when
        // a pending request already exists for this student.
        Task<DeleteRequestResponseDto> CreateAsync(DeleteRequestCreateDto createDto);
        Task<IEnumerable<DeleteRequestResponseDto>> GetForStudentAsync(int studentId);
    }
}
