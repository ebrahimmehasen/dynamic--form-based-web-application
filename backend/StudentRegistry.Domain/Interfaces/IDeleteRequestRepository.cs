using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IDeleteRequestRepository
    {
        Task<DeleteRequest?> GetByIdAsync(int id);
        Task<DeleteRequest?> GetPendingForStudentAsync(int studentId);
        Task<IEnumerable<DeleteRequest>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<DeleteRequest>> GetByStudentIdsAsync(IEnumerable<int> studentIds);
        Task<IEnumerable<DeleteRequest>> GetAllAsync(string? status = null);
        Task AddAsync(DeleteRequest request);
    }
}
