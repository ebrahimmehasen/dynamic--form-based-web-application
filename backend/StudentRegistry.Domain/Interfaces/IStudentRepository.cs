using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(int id);
        Task<Student?> GetByNationalIdAsync(string nationalId);
        Task<Student?> GetBySubmissionTokenAsync(string submissionToken);
        Task<IEnumerable<Student>> GetAllAsync();
        Task<IEnumerable<Student>> SearchAsync(string? query, int take = 500);
        Task<(IEnumerable<Student> Items, int TotalCount)> SearchPagedAsync(string? query, int page, int pageSize);
        Task AddAsync(Student student);
        void Update(Student student);
        void Delete(Student student);
    }
}
