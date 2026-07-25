using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StudentRegistryDbContext _context;
        private IStudentRepository? _students;
        private IReviewNoteRepository? _reviewNotes;

        public UnitOfWork(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public IStudentRepository Students => _students ??= new StudentRepository(_context);
        public IReviewNoteRepository ReviewNotes => _reviewNotes ??= new ReviewNoteRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
