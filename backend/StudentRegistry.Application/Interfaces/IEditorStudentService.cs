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
    }
}
