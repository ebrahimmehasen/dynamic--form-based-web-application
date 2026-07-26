using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly StudentRegistryDbContext _context;

        public UserRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> AnyAsync()
        {
            return await _context.Users.AnyAsync();
        }
    }
}
