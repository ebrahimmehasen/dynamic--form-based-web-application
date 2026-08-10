using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class CertificationDisplaySettingRepository : ICertificationDisplaySettingRepository
    {
        private readonly StudentRegistryDbContext _context;

        public CertificationDisplaySettingRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CertificationDisplaySetting>> GetAllAsync()
        {
            return await _context.CertificationDisplaySettings.AsNoTracking().ToListAsync();
        }

        public async Task<CertificationDisplaySetting?> GetByKeyAsync(string certificationKey)
        {
            return await _context.CertificationDisplaySettings
                .FirstOrDefaultAsync(s => s.CertificationKey == certificationKey);
        }

        public async Task AddAsync(CertificationDisplaySetting setting)
        {
            await _context.CertificationDisplaySettings.AddAsync(setting);
        }
    }
}
