using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IEditorStudentService
    {
        Task<PagedResultDto<EditorStudentListItemDto>> SearchAsync(string? query, int page, int pageSize);
        Task<StudentResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<AuditLogEntryDto>> GetAuditLogAsync(int studentId);

        // Throws KeyNotFoundException if the student doesn't exist, ArgumentException if this
        // certificate type has nothing to recalculate or the edited grades are no longer valid.
        Task<StudentResponseDto> RecalculateAsync(int studentId, string editorUsername);
    }
}
