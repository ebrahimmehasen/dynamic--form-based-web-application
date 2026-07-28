using System.Collections.Generic;

namespace StudentRegistry.Application.DTOs
{
    public class CertificateCountDto
    {
        public string Certification { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class OtherCertificateCountDto
    {
        public string CertificateName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalStudents { get; set; }
        public List<CertificateCountDto> StudentsPerCertificate { get; set; } = new();

        public int TotalOtherCertificateStudents { get; set; }
        public int DistinctOtherCertificates { get; set; }
        public List<OtherCertificateCountDto> OtherCertificates { get; set; } = new();

        public int TotalComments { get; set; }
        public int ReviewNotesCount { get; set; }
        public int FieldCommentsCount { get; set; }
        public List<CertificateCountDto> CommentsPerCertificate { get; set; } = new();

        public int EligibleCount { get; set; }
        public int NotEligibleCount { get; set; }

        // Populated only on the initial, unfiltered load — used to build the certificate filter dropdown.
        public List<string>? AvailableCertifications { get; set; }
    }
}
