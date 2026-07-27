using StudentRegistry.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IPendingReviewService
    {
        // Throws KeyNotFoundException when the student doesn't exist, InvalidOperationException when
        // this student is already pending review.
        Task<PendingReviewResponseDto> FlagAsync(int studentId, string flaggedBy);

        // Throws KeyNotFoundException when the student doesn't exist or has no pending review to resolve.
        Task<PendingReviewResponseDto> ResolveAsync(int studentId, string resolvedBy);

        Task<IEnumerable<PendingReviewResponseDto>> GetForStudentAsync(int studentId);
    }
}
