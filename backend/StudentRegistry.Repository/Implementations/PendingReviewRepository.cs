using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class PendingReviewRepository : IPendingReviewRepository
    {
        private readonly StudentRegistryDbContext _context;

        public PendingReviewRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        // Tracked (no AsNoTracking): the resolve flow mutates Status/ResolvedBy/ResolvedAt on the
        // returned entity and relies on the change tracker for the subsequent SaveChanges.
        public async Task<PendingReview?> GetPendingForStudentAsync(int studentId)
        {
            return await _context.PendingReviews
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == "pending");
        }

        public async Task<IEnumerable<PendingReview>> GetByStudentIdAsync(int studentId)
        {
            return await _context.PendingReviews
                .AsNoTracking()
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.FlaggedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PendingReview>> GetPendingByStudentIdsAsync(IEnumerable<int> studentIds)
        {
            // EF.Constant forces this list to be inlined as SQL literals instead of EF Core 8's default
            // OPENJSON-based parameterization, which this SQL Server instance's compatibility level rejects
            // (mirrors DeleteRequestRepository.GetByStudentIdsAsync).
            var idsList = studentIds as List<int> ?? studentIds.ToList();
            return await _context.PendingReviews
                .AsNoTracking()
                .Where(p => p.Status == "pending" && EF.Constant(idsList).Contains(p.StudentId))
                .ToListAsync();
        }

        public async Task AddAsync(PendingReview review)
        {
            await _context.PendingReviews.AddAsync(review);
        }
    }
}
