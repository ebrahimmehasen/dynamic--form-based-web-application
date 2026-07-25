using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
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

        public async Task AddAsync(ReviewNote note)
        {
            await _context.ReviewNotes.AddAsync(note);
        }
    }
}
