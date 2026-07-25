using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IReviewNoteRepository
    {
        Task<IEnumerable<ReviewNote>> GetByStudentIdAsync(int studentId);
        Task AddAsync(ReviewNote note);
    }
}
