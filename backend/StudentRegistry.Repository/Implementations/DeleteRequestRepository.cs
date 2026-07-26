using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class DeleteRequestRepository : IDeleteRequestRepository
    {
        private readonly StudentRegistryDbContext _context;

        public DeleteRequestRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        // Tracked (no AsNoTracking): callers mutate Status/ReviewedBy/ReviewedAt on the returned
        // entity and rely on the change tracker for the subsequent SaveChanges (approve/reject flows).
        public async Task<DeleteRequest?> GetByIdAsync(int id)
        {
            return await _context.DeleteRequests.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<DeleteRequest?> GetPendingForStudentAsync(int studentId)
        {
            return await _context.DeleteRequests
                .FirstOrDefaultAsync(d => d.StudentId == studentId && d.Status == "pending");
        }

        public async Task<IEnumerable<DeleteRequest>> GetByStudentIdAsync(int studentId)
        {
            return await _context.DeleteRequests
                .AsNoTracking()
                .Where(d => d.StudentId == studentId)
                .OrderByDescending(d => d.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeleteRequest>> GetByStudentIdsAsync(IEnumerable<int> studentIds)
        {
            // EF.Constant forces this list to be inlined as SQL literals instead of EF Core 8's default
            // OPENJSON-based parameterization, which this SQL Server instance's compatibility level rejects.
            var idsList = studentIds as List<int> ?? studentIds.ToList();
            return await _context.DeleteRequests
                .AsNoTracking()
                .Where(d => d.StudentId != null && EF.Constant(idsList).Contains(d.StudentId.Value))
                .ToListAsync();
        }

        public async Task<IEnumerable<DeleteRequest>> GetAllAsync(string? status = null)
        {
            var query = _context.DeleteRequests.AsNoTracking().Include(d => d.Student).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(d => d.Status == status);
            }
            return await query.OrderByDescending(d => d.RequestedAt).ToListAsync();
        }

        public async Task AddAsync(DeleteRequest request)
        {
            await _context.DeleteRequests.AddAsync(request);
        }
    }
}
