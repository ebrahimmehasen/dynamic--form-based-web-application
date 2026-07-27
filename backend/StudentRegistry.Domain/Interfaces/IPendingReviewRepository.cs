using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IPendingReviewRepository
    {
        Task<PendingReview?> GetPendingForStudentAsync(int studentId);
        Task<IEnumerable<PendingReview>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<PendingReview>> GetPendingByStudentIdsAsync(IEnumerable<int> studentIds);
        Task AddAsync(PendingReview review);
    }
}
