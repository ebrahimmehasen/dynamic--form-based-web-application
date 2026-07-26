using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class FieldCommentRepository : IFieldCommentRepository
    {
        private readonly StudentRegistryDbContext _context;

        public FieldCommentRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        // Tracked (no AsNoTracking): callers mutate Status/UpdatedAt on the returned entity and rely
        // on the change tracker for the subsequent SaveChanges (dismiss / approve-as-edit flows).
        public async Task<FieldComment?> GetByIdAsync(int id)
        {
            return await _context.FieldComments.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<FieldComment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.FieldComments
                .AsNoTracking()
                .Where(f => f.StudentId == studentId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldComment>> GetByStudentIdsAsync(IEnumerable<int> studentIds)
        {
            // EF.Constant forces this list to be inlined as SQL literals instead of EF Core 8's default
            // OPENJSON-based parameterization, which this SQL Server instance's compatibility level rejects.
            var idsList = studentIds as List<int> ?? studentIds.ToList();
            return await _context.FieldComments
                .AsNoTracking()
                .Where(f => EF.Constant(idsList).Contains(f.StudentId))
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldComment>> GetUnreviewedAsync()
        {
            return await _context.FieldComments
                .AsNoTracking()
                .Include(f => f.Student)
                .Where(f => f.Status == "unreviewed")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldComment>> GetResolvedAsync()
        {
            return await _context.FieldComments
                .AsNoTracking()
                .Include(f => f.Student)
                .Where(f => f.Status != "unreviewed")
                .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreviewedCountAsync()
        {
            return await _context.FieldComments.CountAsync(f => f.Status == "unreviewed");
        }

        public async Task<IEnumerable<FieldComment>> GetAllAsync()
        {
            return await _context.FieldComments
                .AsNoTracking()
                .Include(f => f.Student)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(FieldComment comment)
        {
            await _context.FieldComments.AddAsync(comment);
        }
    }
}
