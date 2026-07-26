using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        Task<bool> AnyAsync();
    }
}
