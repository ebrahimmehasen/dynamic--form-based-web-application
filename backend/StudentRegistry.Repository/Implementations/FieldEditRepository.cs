using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class FieldEditRepository : IFieldEditRepository
    {
        private readonly StudentRegistryDbContext _context;

        public FieldEditRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FieldEdit>> GetByStudentIdAsync(int studentId)
        {
            return await _context.FieldEdits
                .AsNoTracking()
                .Where(f => f.StudentId == studentId)
                .OrderByDescending(f => f.EditedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldEdit>> GetByStudentIdsAsync(IEnumerable<int> studentIds)
        {
            // EF.Constant forces this list to be inlined as SQL literals instead of EF Core 8's default
            // OPENJSON-based parameterization, which this SQL Server instance's compatibility level rejects.
            var idsList = studentIds as List<int> ?? studentIds.ToList();
            return await _context.FieldEdits
                .AsNoTracking()
                .Where(f => EF.Constant(idsList).Contains(f.StudentId))
                .ToListAsync();
        }

        public async Task<IEnumerable<FieldEdit>> GetAllAsync(bool fromCommentOnly = false)
        {
            var query = _context.FieldEdits.AsNoTracking().Include(f => f.Student).AsQueryable();
            if (fromCommentOnly)
            {
                query = query.Where(f => f.SourceCommentId != null);
            }
            return await query.OrderByDescending(f => f.EditedAt).ToListAsync();
        }

        public async Task AddAsync(FieldEdit edit)
        {
            await _context.FieldEdits.AddAsync(edit);
        }
    }
}
