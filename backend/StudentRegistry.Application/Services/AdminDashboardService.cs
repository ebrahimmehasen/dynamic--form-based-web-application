using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    // Powers the Admin Dashboard. "Comments" combines both comment systems in this app: the
    // Viewer page's append-only ReviewNotes, and the Editor page's FieldComments — an admin asking
    // "how many comments exist" should see the full picture, not just one subsystem.
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminDashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardStatsDto> GetStatsAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var dashboard = _unitOfWork.Dashboard;

            var totalStudents = await dashboard.GetTotalStudentsAsync(startDate, endDate, certification);
            var studentsPerCert = await dashboard.GetStudentsPerCertificateAsync(startDate, endDate, certification);
            var otherCerts = await dashboard.GetOtherCertificatesAsync(startDate, endDate, certification);

            var reviewNotesCount = await dashboard.GetReviewNotesCountAsync(startDate, endDate, certification);
            var fieldCommentsCount = await dashboard.GetFieldCommentsCountAsync(startDate, endDate, certification);
            var reviewNotesPerCert = await dashboard.GetReviewNotesPerCertificateAsync(startDate, endDate, certification);
            var fieldCommentsPerCert = await dashboard.GetFieldCommentsPerCertificateAsync(startDate, endDate, certification);

            var eligibleCount = await dashboard.GetEligibleCountAsync(startDate, endDate, certification);
            var notEligibleCount = await dashboard.GetNotEligibleCountAsync(startDate, endDate, certification);

            var combinedComments = new Dictionary<string, int>();
            foreach (var item in reviewNotesPerCert)
            {
                combinedComments[item.Certification] = combinedComments.GetValueOrDefault(item.Certification) + item.Count;
            }
            foreach (var item in fieldCommentsPerCert)
            {
                combinedComments[item.Certification] = combinedComments.GetValueOrDefault(item.Certification) + item.Count;
            }

            var result = new DashboardStatsDto
            {
                TotalStudents = totalStudents,
                StudentsPerCertificate = studentsPerCert
                    .Select(x => new CertificateCountDto { Certification = x.Certification, Count = x.Count })
                    .ToList(),

                OtherCertificates = otherCerts
                    .Select(x => new OtherCertificateCountDto { CertificateName = x.CertificateName, Count = x.Count })
                    .ToList(),
                TotalOtherCertificateStudents = otherCerts.Sum(x => x.Count),
                DistinctOtherCertificates = otherCerts.Count,

                ReviewNotesCount = reviewNotesCount,
                FieldCommentsCount = fieldCommentsCount,
                TotalComments = reviewNotesCount + fieldCommentsCount,
                CommentsPerCertificate = combinedComments
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => new CertificateCountDto { Certification = kv.Key, Count = kv.Value })
                    .ToList(),

                EligibleCount = eligibleCount,
                NotEligibleCount = notEligibleCount
            };

            // Always the full, unfiltered list (independent of the current filters) so the dropdown
            // never shrinks to just whatever's currently selected.
            result.AvailableCertifications = await dashboard.GetDistinctCertificationsAsync();

            return result;
        }
    }
}
