using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IFieldCommentRepository
    {
        Task<FieldComment?> GetByIdAsync(int id);
        Task<IEnumerable<FieldComment>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<FieldComment>> GetByStudentIdsAsync(IEnumerable<int> studentIds);
        Task<IEnumerable<FieldComment>> GetUnreviewedAsync();
        Task<IEnumerable<FieldComment>> GetResolvedAsync();
        Task<int> GetUnreviewedCountAsync();
        Task<IEnumerable<FieldComment>> GetAllAsync();
        Task AddAsync(FieldComment comment);
    }
}
