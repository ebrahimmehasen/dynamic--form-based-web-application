using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class ReviewNoteRepository : IReviewNoteRepository
    {
        private readonly StudentRegistryDbContext _context;

        public ReviewNoteRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewNote>> GetByStudentIdAsync(int studentId)
        {
            return await _context.ReviewNotes
                .AsNoTracking()
                .Where(n => n.StudentId == studentId)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewNote>> GetByStudentIdsAsync(IEnumerable<int> studentIds)
        {
            // EF.Constant forces this list to be inlined as SQL literals instead of EF Core 8's default
            // OPENJSON-based parameterization, which this SQL Server instance's compatibility level rejects.
            var idsList = studentIds as List<int> ?? studentIds.ToList();
            return await _context.ReviewNotes
                .AsNoTracking()
                .Where(n => EF.Constant(idsList).Contains(n.StudentId))
                .ToListAsync();
        }

        public async Task AddAsync(ReviewNote note)
        {
            await _context.ReviewNotes.AddAsync(note);
        }
    }
}
