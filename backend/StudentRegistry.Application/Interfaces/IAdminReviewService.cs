using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IAdminReviewService
    {
        Task<IEnumerable<FieldEditResponseDto>> GetFieldEditsAsync(bool fromCommentOnly);
        Task<IEnumerable<FieldCommentResponseDto>> GetFieldCommentsAsync();
        Task<IEnumerable<DeleteRequestResponseDto>> GetDeleteRequestsAsync(string? status);

        // Both return null when the request doesn't exist or is no longer pending.
        Task<DeleteRequestResponseDto?> ApproveDeleteAsync(int id);
        Task<DeleteRequestResponseDto?> RejectDeleteAsync(int id);
    }
}
