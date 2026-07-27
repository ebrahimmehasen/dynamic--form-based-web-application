using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);
        Task AddAsync(User user);
        void Delete(User user);
        Task<bool> AnyAsync();
    }
}
