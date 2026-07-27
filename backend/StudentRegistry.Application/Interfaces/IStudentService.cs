using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IStudentService
    {
        Task<StudentResponseDto?> GetStudentByIdAsync(int id);
        Task<StudentResponseDto?> GetStudentByNationalIdAsync(string nationalId);
        Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync();
        Task<IEnumerable<StudentListItemDto>> SearchStudentsAsync(string? query);
        Task<StudentResponseDto> RegisterStudentAsync(StudentCreateDto createDto);

        // Re-runs the exact same per-certificate formulas used at registration against whatever raw
        // grade rows currently exist in the DB (which may since have been edited via FieldEdits), and
        // overwrites the certificate's totals accordingly. Every changed total field is logged as its
        // own FieldEdit (Source = "recalculate") under the given editor's real username.
        Task<StudentResponseDto> RecalculateStudentTotalsAsync(int studentId, string editorUsername);
    }
}
