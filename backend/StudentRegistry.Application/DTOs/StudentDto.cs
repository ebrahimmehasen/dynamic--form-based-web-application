using System;
using System.Collections.Generic;

namespace StudentRegistry.Application.DTOs
{
    public class StudentCreateDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentNameEn { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string WishCollege { get; set; } = string.Empty;   // "الرغبة" — desired college, selection-only
        public string? WishProgram { get; set; }   // desired program, when applicable to the college
        public int GraduationYear { get; set; }   // "سنة التخرج" — selection-only, 2022-2026
        public string Gender { get; set; } = string.Empty;   // ذكر / أنثى
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string GuardianNationalId { get; set; } = string.Empty;
        public string GuardianOccupation { get; set; } = string.Empty;
        public string GuardianPhone { get; set; } = string.Empty;
        public string GuardianRelation { get; set; } = string.Empty;
        public string AddressGov { get; set; } = string.Empty;
        public string AddressCenter { get; set; } = string.Empty;
        public string? AddressVillage { get; set; }
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressBuilding { get; set; } = string.Empty;
        public string? AddressFloor { get; set; }
        public string Certification { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;

        // Base64 encoded image string from frontend
        public string Photo { get; set; } = string.Empty;

        // Saudi specific fields
        public string? YearsCount { get; set; }
        public List<SaudiGradeCreateDto>? SaudiGrades { get; set; }
        public decimal? AptitudeScore { get; set; }

        // IG specific fields
        public string? IgProgram { get; set; }
        public decimal? Factor { get; set; }
        public decimal? SportsBonus { get; set; }
        public List<IgGradeCountCreateDto>? IgGradeCounts { get; set; }

        // Standard certificate fields
        public string? YearOfStudy { get; set; }
        public List<StandardGradeCreateDto>? StandardGrades { get; set; }

        // Kuwaiti specific fields
        public KuwaitiDataCreateDto? KuwaitiData { get; set; }

        // Qatari specific fields
        public QatariDataCreateDto? QatariData { get; set; }

        // Omani specific fields
        public OmaniDataCreateDto? OmaniData { get; set; }

        // Yemeni specific fields
        public YemeniDataCreateDto? YemeniData { get; set; }

        // Bahraini specific fields
        public BahrainiDataCreateDto? BahrainiData { get; set; }

        // Palestinian specific fields
        public PalestinianDataCreateDto? PalestinianData { get; set; }

        // "أخرى" (other) specific fields
        public OtherDataCreateDto? OtherData { get; set; }

        // Egyptian Thanaweya Amma specific fields
        public EgyptianDataCreateDto? EgyptianData { get; set; }

        // Azhar Thanaweya specific fields
        public AzharDataCreateDto? AzharData { get; set; }

        // Emirati specific fields
        public EmiratiDataCreateDto? EmiratiData { get; set; }

        // American Diploma specific fields
        public AmericanDiplomaDataCreateDto? AmericanDiplomaData { get; set; }
    }

    public class KuwaitiDataCreateDto
    {
        // "Two Years" (grade 11 + 12) or "Three Years" (grade 10 + 11 + 12) — mirrors Saudi's YearsCount.
        public string YearsCount { get; set; } = string.Empty;
        public bool HasSecondAttempt { get; set; }

        // Weight (%) for each included grade level, as printed on the student's own certificate.
        // Grade10Weight is only used/required when YearsCount is "Three Years".
        public decimal? Grade10Weight { get; set; }
        public decimal? Grade11Weight { get; set; }
        public decimal? Grade12Weight { get; set; }

        public List<KuwaitiSubjectGradeCreateDto>? Grade10Subjects { get; set; }
        public List<KuwaitiSubjectGradeCreateDto>? Grade11Subjects { get; set; }
        public List<KuwaitiSubjectGradeCreateDto>? Grade12Subjects { get; set; }
    }

    public class KuwaitiSubjectGradeCreateDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal Obtained { get; set; }
        // Max mark is fixed server-side (KuwaitiConstants) — never accepted from the client.
    }

    public class QatariDataCreateDto
    {
        // The 7 scientific-track subjects — max mark is fixed server-side (§1.2), never client-supplied.
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
    }

    public class OmaniDataCreateDto
    {
        // The 7 counted subjects — max mark is fixed server-side (§1.2), never client-supplied.
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
    }

    public class YemeniDataCreateDto
    {
        // The 6 counted subjects — max mark is fixed server-side (§1.2), never client-supplied.
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
    }

    public class BahrainiDataCreateDto
    {
        // Subject list varies by Track (علمي: 7 subjects / أدبي: 8 subjects) — max mark is fixed
        // server-side (§SingleYearFixedTotalConstants), never client-supplied.
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
    }

    // Percentage-in only — no subjects, no max marks, no denominator. The student types their
    // Tawjihi percentage directly and the site converts it.
    public class PalestinianDataCreateDto
    {
        public decimal Percentage { get; set; }
        public string Branch { get; set; } = string.Empty;   // علمي / أدبي — recorded only, never forks the calculation.
    }

    // Percentage-in only, free-text certificate name, no track — for certificates not otherwise
    // supported in this system.
    public class OtherDataCreateDto
    {
        public string CertificateName { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
    }

    // Track (Track on the parent DTO) + subject system determine the exact subject set — max marks
    // are fixed server-side (§EgyptianConstants), never client-supplied.
    public class EgyptianDataCreateDto
    {
        public string SubjectSystem { get; set; } = string.Empty;   // قديم / حديث
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
    }

    // Section (قسم) is the parent DTO's Track — max marks are fixed server-side
    // (§AzharConstants), never client-supplied.
    public class AzharDataCreateDto
    {
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
    }

    // Core subjects (5) are always required; Optional subjects (الكيمياء/العلوم الصحية/الأحياء) are
    // counted only if the student includes a row for them — max mark is fixed server-side
    // (§SingleYearFixedTotalConstants), never client-supplied.
    public class EmiratiDataCreateDto
    {
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
    }

    // §American Diploma — the student types in their own best 8 subject names (no fixed list);
    // each is fixed at 100 max, just like the single-year-fixed-total family. SatI is always
    // required. SatII is shown for every college where AmericanDiplomaConstants.
    // IsSatIIApplicable(WishCollege) is true (never for تجارة); whether it's mandatory there depends
    // on AmericanDiplomaConstants.IsSatIIMandatory(WishCollege, StudiedAdvancedMath) — the medical
    // group is always optional, the engineering group is mandatory unless StudiedAdvancedMath is
    // true. Whenever SatII IS provided (mandatory or optionally filled in), its two subjects are
    // still required and validated.
    public class AmericanDiplomaDataCreateDto
    {
        public List<SingleYearSubjectMarkCreateDto>? Subjects { get; set; }
        public int SatI { get; set; }
        public int? SatII { get; set; }
        public string? SatIISubject1 { get; set; }
        public string? SatIISubject2 { get; set; }
        public bool StudiedAdvancedMath { get; set; }   // only meaningful for the engineering group
    }

    // Shared by Qatari, Omani, Yemeni, Bahraini and Emirati (all single-year, fixed-100-per-subject certificates).
    public class SingleYearSubjectMarkCreateDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal Mark { get; set; }
    }

    public class SaudiGradeCreateDto
    {
        public string YearLabel { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal Achieved { get; set; }
        public decimal Weighted { get; set; }
    }

    public class IgGradeCountCreateDto
    {
        public string GradeType { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StandardGradeCreateDto
    {
        public string YearOfStudy { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public decimal WeightedPercentage { get; set; }
    }

    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNameEn { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string WishCollege { get; set; } = string.Empty;
        public string? WishProgram { get; set; }
        public int GraduationYear { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string GuardianNationalId { get; set; } = string.Empty;
        public string GuardianOccupation { get; set; } = string.Empty;
        public string GuardianPhone { get; set; } = string.Empty;
        public string GuardianRelation { get; set; } = string.Empty;
        public string AddressGov { get; set; } = string.Empty;
        public string AddressCenter { get; set; } = string.Empty;
        public string? AddressVillage { get; set; }
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressBuilding { get; set; } = string.Empty;
        public string? AddressFloor { get; set; }
        public string Certification { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }

        // Associated records depending on certificate type
        public SaudiTotalsResponseDto? SaudiTotals { get; set; }
        public List<SaudiGradeResponseDto>? SaudiGrades { get; set; }
        public IgGradesResponseDto? IgGrades { get; set; }
        public List<StandardGradeResponseDto>? StandardGrades { get; set; }
        public KuwaitiTotalsResponseDto? KuwaitiTotals { get; set; }
        public List<KuwaitiGradeResponseDto>? KuwaitiGrades { get; set; }
        public QatariTotalsResponseDto? QatariTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? QatariGrades { get; set; }
        public OmaniTotalsResponseDto? OmaniTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? OmaniGrades { get; set; }
        public YemeniTotalsResponseDto? YemeniTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? YemeniGrades { get; set; }
        public BahrainiTotalsResponseDto? BahrainiTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? BahrainiGrades { get; set; }
        public PalestinianTotalsResponseDto? PalestinianTotals { get; set; }
        public OtherTotalsResponseDto? OtherTotals { get; set; }
        public EgyptianTotalsResponseDto? EgyptianTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? EgyptianGrades { get; set; }
        public AzharTotalsResponseDto? AzharTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? AzharGrades { get; set; }
        public EmiratiTotalsResponseDto? EmiratiTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? EmiratiGrades { get; set; }
        public AmericanDiplomaTotalsResponseDto? AmericanDiplomaTotals { get; set; }
        public List<SingleYearSubjectMarkResponseDto>? AmericanDiplomaGrades { get; set; }
    }

    public class KuwaitiTotalsResponseDto
    {
        public string YearsCount { get; set; } = string.Empty;
        public decimal? Grade10Percentage { get; set; }
        public decimal? Grade11Percentage { get; set; }
        public decimal Grade12Percentage { get; set; }
        public decimal? Grade10Weight { get; set; }
        public decimal? Grade11Weight { get; set; }
        public decimal Grade12Weight { get; set; }
        // The final equivalent percentage (0-100) — the percentage form of EquivalentTotal (out of 410).
        public decimal FinalPercentage { get; set; }
        public decimal EquivalentTotal { get; set; }
        public bool HasSecondAttempt { get; set; }
        public string Disclaimer { get; set; } = string.Empty;
        public string? SecondAttemptWarning { get; set; }
    }

    public class KuwaitiGradeResponseDto
    {
        public int GradeLevel { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public decimal Obtained { get; set; }
        public decimal MaxMark { get; set; }
    }

    public class QatariTotalsResponseDto
    {
        public decimal FinalTotal { get; set; }   // out of 700
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // المجموع الاعتباري (المجموع المصري), out of 410
        public string Disclaimer { get; set; } = string.Empty;
    }

    public class OmaniTotalsResponseDto
    {
        public decimal FinalTotal { get; set; }   // out of 700
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // المجموع الاعتباري (المجموع المصري), out of 410
        public string Disclaimer { get; set; } = string.Empty;
    }

    public class YemeniTotalsResponseDto
    {
        public decimal FinalTotal { get; set; }   // out of 600
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // المجموع الاعتباري (المجموع المصري), out of 410
        public string Disclaimer { get; set; } = string.Empty;
    }

    public class BahrainiTotalsResponseDto
    {
        public string Track { get; set; } = string.Empty;
        public decimal FinalTotal { get; set; }
        public decimal TotalMax { get; set; }   // 700 (علمي) or 800 (أدبي)
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // out of 410
        public string Disclaimer { get; set; } = string.Empty;
    }

    public class PalestinianTotalsResponseDto
    {
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // المجموع الاعتباري (المجموع المصري), out of 410
        public string Branch { get; set; } = string.Empty;
        public string Disclaimer { get; set; } = string.Empty;
    }

    public class OtherTotalsResponseDto
    {
        public string CertificateName { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string Disclaimer { get; set; } = string.Empty;
    }

    // This IS the Egyptian target certificate itself — no equivalent-total conversion, unlike every
    // other certificate in this system.
    public class EgyptianTotalsResponseDto
    {
        public string Track { get; set; } = string.Empty;
        public string SubjectSystem { get; set; } = string.Empty;
        public decimal FinalTotal { get; set; }
        public decimal Denominator { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AzharTotalsResponseDto
    {
        public string Section { get; set; } = string.Empty;
        public decimal FinalTotal { get; set; }
        public decimal Denominator { get; set; }
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // المجموع الاعتباري (المجموع المصري), out of 410
    }

    // Denominator varies (500-800) depending on how many optional subjects were entered.
    public class EmiratiTotalsResponseDto
    {
        public decimal FinalTotal { get; set; }
        public decimal Denominator { get; set; }
        public decimal Percentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // المجموع الاعتباري (المجموع المصري), out of 410
        public string Disclaimer { get; set; } = string.Empty;
    }

    // No EquivalentTotal — admission depends on BasePercentage + SatI + SatII together, never a
    // single combined number (unlike every other certificate in this system).
    public class AmericanDiplomaTotalsResponseDto
    {
        public decimal AverageScore { get; set; }
        public decimal BasePercentage { get; set; }   // out of 40
        public int SatI { get; set; }
        public int? SatII { get; set; }
        public string? SatIISubject1 { get; set; }
        public string? SatIISubject2 { get; set; }
        public bool StudiedAdvancedMath { get; set; }
        public bool SatIBelowMinimum { get; set; }
        public bool SatIIBelowMinimum { get; set; }
        public string AdmissionNote { get; set; } = string.Empty;
        public string Disclaimer { get; set; } = string.Empty;
    }

    // Shared by Qatari, Omani, Yemeni, Bahraini and Emirati grade lists.
    public class SingleYearSubjectMarkResponseDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public decimal Mark { get; set; }
    }

    public class SaudiTotalsResponseDto
    {
        public string YearsCount { get; set; } = string.Empty;
        public decimal TotalAchieved { get; set; }
        public decimal TotalWeighted { get; set; }
        public int TotalCoefficients { get; set; }
        public decimal SchoolPercentage { get; set; }
        public decimal AptitudeScore { get; set; }
        public decimal FinalPercentage { get; set; }
        public decimal EquivalentTotal { get; set; }   // المجموع الاعتباري (المجموع المصري), out of 410
    }

    public class SaudiGradeResponseDto
    {
        public string YearLabel { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Coefficient { get; set; }
        public decimal Achieved { get; set; }
        public decimal Weighted { get; set; }
    }

    public class IgGradesResponseDto
    {
        public string IgProgram { get; set; } = string.Empty;
        public decimal Factor { get; set; }
        public decimal SportsBonus { get; set; }
        public decimal ScorePercentage { get; set; }
        public decimal GovernmentScore { get; set; }
        public List<IgGradeCountResponseDto> GradeCounts { get; set; } = new List<IgGradeCountResponseDto>();
    }

    public class IgGradeCountResponseDto
    {
        public string GradeType { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StandardGradeResponseDto
    {
        public string YearOfStudy { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public decimal WeightedPercentage { get; set; }
        public decimal Achieved { get; set; }
    }
}
