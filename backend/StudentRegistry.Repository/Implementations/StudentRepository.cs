using Microsoft.EntityFrameworkCore;
using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Student?> GetBySubmissionTokenAsync(string submissionToken)
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
                .FirstOrDefaultAsync(s => s.SubmissionToken == submissionToken);
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.SaudiTotals)
                .Include(s => s.IgGrades)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> SearchAsync(string? query, int take = 500)
        {
            var students = _context.Students.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim();
                students = students.Where(s =>
                    s.StudentName.Contains(term) ||
                    s.StudentNameEn.Contains(term) ||
                    s.NationalId.Contains(term) ||
                    s.Phone.Contains(term) ||
                    s.Email.Contains(term) ||
                    s.GuardianName.Contains(term) ||
                    s.AddressGov.Contains(term) ||
                    s.AddressCenter.Contains(term) ||
                    s.Certification.Contains(term) ||
                    s.Track.Contains(term) ||
                    s.WishCollege.Contains(term) ||
                    s.GraduationYear.ToString().Contains(term));
            }

            return await students
                .OrderByDescending(s => s.SubmittedAt)
                .Take(take)
                .ToListAsync();
        }

        // Additive, DB-level paginated search used only by the Student Records Editor page — leaves
        // SearchAsync (used by the read-only Student Records Review page) untouched.
        public async Task<(IEnumerable<Student> Items, int TotalCount)> SearchPagedAsync(string? query, int page, int pageSize)
        {
            var students = _context.Students.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim();
                students = students.Where(s =>
                    s.StudentName.Contains(term) ||
                    s.StudentNameEn.Contains(term) ||
                    s.NationalId.Contains(term) ||
                    s.Phone.Contains(term) ||
                    s.Email.Contains(term) ||
                    s.GuardianName.Contains(term) ||
                    s.AddressGov.Contains(term) ||
                    s.AddressCenter.Contains(term) ||
                    s.Certification.Contains(term) ||
                    s.Track.Contains(term) ||
                    s.WishCollege.Contains(term) ||
                    s.GraduationYear.ToString().Contains(term));
            }

            var totalCount = await students.CountAsync();
            var items = await students
                .OrderByDescending(s => s.SubmittedAt)
                .Skip(Math.Max(0, page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
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
