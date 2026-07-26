using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IFieldEditRepository
    {
        Task<IEnumerable<FieldEdit>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<FieldEdit>> GetByStudentIdsAsync(IEnumerable<int> studentIds);
        Task<IEnumerable<FieldEdit>> GetAllAsync(bool fromCommentOnly = false);
        Task AddAsync(FieldEdit edit);
    }
}
