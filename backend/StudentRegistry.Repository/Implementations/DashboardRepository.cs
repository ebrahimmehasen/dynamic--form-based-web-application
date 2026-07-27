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
    public class DashboardRepository : IDashboardRepository
    {
        private readonly StudentRegistryDbContext _context;

        public DashboardRepository(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalStudentsAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var query = FilterStudents(_context.Students.AsNoTracking(), startDate, endDate, certification);
            return await query.CountAsync();
        }

        public async Task<List<CertificateCount>> GetStudentsPerCertificateAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var query = FilterStudents(_context.Students.AsNoTracking(), startDate, endDate, certification);
            var results = await query
                .GroupBy(s => s.Certification)
                .Select(g => new { Certification = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
            return results.Select(x => new CertificateCount(x.Certification, x.Count)).ToList();
        }

        // "Other" certificates are recorded as free text on OtherStudentTotals.CertificateName —
        // this is the exact list of specific external certificates students entered.
        public async Task<List<OtherCertificateCount>> GetOtherCertificatesAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var query = FilterStudents(_context.Students.AsNoTracking(), startDate, endDate, certification)
                .Where(s => s.OtherTotals != null);

            var results = await query
                .GroupBy(s => s.OtherTotals!.CertificateName)
                .Select(g => new { CertificateName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
            return results.Select(x => new OtherCertificateCount(x.CertificateName, x.Count)).ToList();
        }

        public async Task<int> GetReviewNotesCountAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var query = FilterReviewNotes(_context.ReviewNotes.AsNoTracking(), startDate, endDate, certification);
            return await query.CountAsync();
        }

        public async Task<int> GetFieldCommentsCountAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var query = FilterFieldComments(_context.FieldComments.AsNoTracking(), startDate, endDate, certification);
            return await query.CountAsync();
        }

        public async Task<List<CertificateCount>> GetReviewNotesPerCertificateAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var query = FilterReviewNotes(_context.ReviewNotes.AsNoTracking(), startDate, endDate, certification);
            var results = await query
                .GroupBy(r => r.Student.Certification)
                .Select(g => new { Certification = g.Key, Count = g.Count() })
                .ToListAsync();
            return results.Select(x => new CertificateCount(x.Certification, x.Count)).ToList();
        }

        public async Task<List<CertificateCount>> GetFieldCommentsPerCertificateAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var query = FilterFieldComments(_context.FieldComments.AsNoTracking(), startDate, endDate, certification);
            var results = await query
                .GroupBy(f => f.Student.Certification)
                .Select(g => new { Certification = g.Key, Count = g.Count() })
                .ToListAsync();
            return results.Select(x => new CertificateCount(x.Certification, x.Count)).ToList();
        }

        public async Task<List<string>> GetDistinctCertificationsAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .Select(s => s.Certification)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        private static IQueryable<Student> FilterStudents(IQueryable<Student> query, DateTime? startDate, DateTime? endDate, string? certification)
        {
            if (startDate.HasValue) query = query.Where(s => s.SubmittedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(s => s.SubmittedAt < endDate.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(certification)) query = query.Where(s => s.Certification == certification);
            return query;
        }

        private static IQueryable<ReviewNote> FilterReviewNotes(IQueryable<ReviewNote> query, DateTime? startDate, DateTime? endDate, string? certification)
        {
            if (startDate.HasValue) query = query.Where(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(r => r.CreatedAt < endDate.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(certification)) query = query.Where(r => r.Student.Certification == certification);
            return query;
        }

        private static IQueryable<FieldComment> FilterFieldComments(IQueryable<FieldComment> query, DateTime? startDate, DateTime? endDate, string? certification)
        {
            if (startDate.HasValue) query = query.Where(f => f.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(f => f.CreatedAt < endDate.Value.AddDays(1));
            if (!string.IsNullOrWhiteSpace(certification)) query = query.Where(f => f.Student.Certification == certification);
            return query;
        }
    }
}
