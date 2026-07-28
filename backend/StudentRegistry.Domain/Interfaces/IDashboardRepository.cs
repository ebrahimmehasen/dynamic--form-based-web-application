using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public record CertificateCount(string Certification, int Count);
    public record OtherCertificateCount(string CertificateName, int Count);

    public interface IDashboardRepository
    {
        // All methods share the same optional filters: startDate/endDate apply to each query's own
        // natural date field (student registration date for student queries, comment creation date
        // for comment queries); certification narrows to a single certificate's data when provided.
        Task<int> GetTotalStudentsAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<List<CertificateCount>> GetStudentsPerCertificateAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<List<OtherCertificateCount>> GetOtherCertificatesAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<int> GetReviewNotesCountAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<int> GetFieldCommentsCountAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<List<CertificateCount>> GetReviewNotesPerCertificateAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<List<CertificateCount>> GetFieldCommentsPerCertificateAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<List<string>> GetDistinctCertificationsAsync();

        // "Eligible"/"NotEligible" match Student.EligibilityStatus exactly; students with a null
        // status (not yet confirmed by an Editor) are excluded from both counts.
        Task<int> GetEligibleCountAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<int> GetNotEligibleCountAsync(DateTime? startDate, DateTime? endDate, string? certification);
        Task<List<Student>> GetStudentsByEligibilityAsync(string eligibilityStatus, DateTime? startDate, DateTime? endDate, string? certification);
        Task<List<Student>> GetAllStudentsFilteredAsync(DateTime? startDate, DateTime? endDate, string? certification);
    }
}
