using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class StudentRepository : IStudentRepository
    {
        private readonly StudentRegistryDbContext _context;

        public StudentRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.SaudiTotals)
                .Include(s => s.SaudiGrades)
                .Include(s => s.IgGrades)
                .Include(s => s.IgGradeCounts)
                .Include(s => s.StandardGrades)
                .Include(s => s.KuwaitiTotals)
                .Include(s => s.QatariTotals)
                .Include(s => s.OmaniTotals)
                .Include(s => s.YemeniTotals)
                .Include(s => s.BahrainiTotals)
                .Include(s => s.PalestinianTotals)
                .Include(s => s.OtherTotals)
                .Include(s => s.EgyptianTotals)
                .Include(s => s.AzharTotals)
                .Include(s => s.EmiratiTotals)
                .Include(s => s.AmericanDiplomaTotals)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student?> GetByNationalIdAsync(string nationalId)
        {
            return await _context.Students
                .Include(s => s.SaudiTotals)
                .Include(s => s.SaudiGrades)
                .Include(s => s.IgGrades)
                .Include(s => s.IgGradeCounts)
                .Include(s => s.StandardGrades)
                .Include(s => s.KuwaitiTotals)
                .Include(s => s.QatariTotals)
                .Include(s => s.OmaniTotals)
                .Include(s => s.YemeniTotals)
                .Include(s => s.BahrainiTotals)
                .Include(s => s.PalestinianTotals)
                .Include(s => s.OtherTotals)
                .Include(s => s.EgyptianTotals)
                .Include(s => s.AzharTotals)
                .Include(s => s.EmiratiTotals)
                .Include(s => s.AmericanDiplomaTotals)
                .FirstOrDefaultAsync(s => s.NationalId == nationalId);
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.SaudiTotals)
                .Include(s => s.IgGrades)
                .ToListAsync();
        }

        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        public void Update(Student student)
        {
            _context.Students.Update(student);
        }

        public void Delete(Student student)
        {
            _context.Students.Remove(student);
        }
    }
}
