using StudentRegistry.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudentRegistry.Application.Editor
{
    public class EditableEntityDescriptor
    {
        public required Type EntityType { get; init; }
        public required string[] EditableProperties { get; init; }
        // Exactly one of these is set: singular entities (Student itself, or a 1:1 "Totals" child)
        // use GetSingle; 1:N "Grades" collections use GetCollection, addressed by the row's own Id.
        public Func<Student, object?>? GetSingle { get; init; }
        public Func<Student, IEnumerable<object>>? GetCollection { get; init; }
        public bool IsCollection => GetCollection != null;
    }

    // Whitelist of every field the Editor page is allowed to write, driving the generic reflection-
    // based edit engine in FieldEditService. Deliberately excludes: Id/StudentId (identity, never
    // user-editable), PhotoPath (files are view-only per spec), SubmittedAt (system timestamp), and
    // GradeLevel (structural discriminator used by MappingProfile/StudentService to tell certificate
    // types apart — editing it could silently reclassify a grade row).
    public static class EditableFieldRegistry
    {
        public static readonly IReadOnlyDictionary<string, EditableEntityDescriptor> Groups =
            new Dictionary<string, EditableEntityDescriptor>
            {
                ["Student"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(Student),
                    EditableProperties = new[]
                    {
                        "StudentName", "StudentNameEn", "NationalId", "WishCollege", "WishProgram",
                        "GraduationYear", "Gender", "Phone", "Email", "BirthCountry", "BirthGovernorate",
                        "BirthCity", "BirthDate", "SchoolName", "GuardianName", "GuardianNationalId",
                        "GuardianOccupation", "GuardianPhone", "GuardianRelation", "AddressGov",
                        "AddressCenter", "AddressVillage", "AddressStreet", "AddressBuilding",
                        "AddressFloor", "Certification", "Track"
                    },
                    GetSingle = s => s
                },
                ["SaudiTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(SaudiStudentTotals),
                    EditableProperties = new[]
                    {
                        "YearsCount", "TotalAchieved", "TotalWeighted", "TotalCoefficients",
                        "SchoolPercentage", "AptitudeScore", "FinalPercentage", "EquivalentTotal"
                    },
                    GetSingle = s => s.SaudiTotals
                },
                ["SaudiGrades"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(SaudiStudentGrades),
                    EditableProperties = new[] { "YearLabel", "SubjectName", "Coefficient", "Achieved", "Weighted" },
                    GetCollection = s => s.SaudiGrades.Cast<object>()
                },
                ["IgGrades"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(IgStudentGrades),
                    EditableProperties = new[] { "IgProgram", "Factor", "SportsBonus", "ScorePercentage", "GovernmentScore" },
                    GetSingle = s => s.IgGrades
                },
                ["IgGradeCounts"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(IgStudentGradeCounts),
                    EditableProperties = new[] { "GradeType", "Grade", "Count" },
                    GetCollection = s => s.IgGradeCounts.Cast<object>()
                },
                // Shared by Kuwaiti/Qatari/Omani/Yemeni/Bahraini/Egyptian/Azhar/Emirati/AmericanDiploma
                // grade rows — they all live in the one StandardStudentGrades table.
                ["StandardGrades"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(StandardStudentGrades),
                    EditableProperties = new[] { "YearOfStudy", "SubjectName", "Grade", "WeightedPercentage", "Achieved", "MaxMark" },
                    GetCollection = s => s.StandardGrades.Cast<object>()
                },
                ["KuwaitiTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(KuwaitiStudentTotals),
                    EditableProperties = new[]
                    {
                        "YearsCount", "Grade10Percentage", "Grade11Percentage", "Grade12Percentage",
                        "Grade10Weight", "Grade11Weight", "Grade12Weight", "FinalPercentage",
                        "EquivalentTotal", "HasSecondAttempt"
                    },
                    GetSingle = s => s.KuwaitiTotals
                },
                ["QatariTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(QatariStudentTotals),
                    EditableProperties = new[] { "FinalTotal", "Percentage", "EquivalentTotal" },
                    GetSingle = s => s.QatariTotals
                },
                ["OmaniTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(OmaniStudentTotals),
                    EditableProperties = new[] { "FinalTotal", "Percentage", "EquivalentTotal" },
                    GetSingle = s => s.OmaniTotals
                },
                ["YemeniTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(YemeniStudentTotals),
                    EditableProperties = new[] { "FinalTotal", "Percentage", "EquivalentTotal" },
                    GetSingle = s => s.YemeniTotals
                },
                ["BahrainiTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(BahrainiStudentTotals),
                    EditableProperties = new[] { "Track", "FinalTotal", "TotalMax", "Percentage", "EquivalentTotal" },
                    GetSingle = s => s.BahrainiTotals
                },
                ["PalestinianTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(PalestinianStudentTotals),
                    EditableProperties = new[] { "Percentage", "EquivalentTotal", "Branch" },
                    GetSingle = s => s.PalestinianTotals
                },
                ["OtherTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(OtherStudentTotals),
                    EditableProperties = new[] { "CertificateName", "Percentage" },
                    GetSingle = s => s.OtherTotals
                },
                ["EgyptianTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(EgyptianStudentTotals),
                    EditableProperties = new[] { "Track", "SubjectSystem", "FinalTotal", "Denominator", "Percentage" },
                    GetSingle = s => s.EgyptianTotals
                },
                ["AzharTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(AzharStudentTotals),
                    EditableProperties = new[] { "Section", "FinalTotal", "Denominator", "Percentage", "EquivalentTotal" },
                    GetSingle = s => s.AzharTotals
                },
                ["EmiratiTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(EmiratiStudentTotals),
                    EditableProperties = new[] { "FinalTotal", "Denominator", "Percentage", "EquivalentTotal" },
                    GetSingle = s => s.EmiratiTotals
                },
                ["AmericanDiplomaTotals"] = new EditableEntityDescriptor
                {
                    EntityType = typeof(AmericanDiplomaStudentTotals),
                    EditableProperties = new[]
                    {
                        "AverageScore", "BasePercentage", "TestType1", "SatI", "ActComposite", "TestType2",
                        "SatII", "ActMath", "SatIISubject1", "SatIISubject2", "StudiedAdvancedMath",
                        "SatIBelowMinimum", "SatIIBelowMinimum"
                    },
                    GetSingle = s => s.AmericanDiplomaTotals
                }
            };

        // Converts an incoming edit's raw string value to the target property's CLR type. Supports
        // every scalar type actually used across the whitelisted fields above (string/int/decimal/
        // bool/DateTime, nullable-aware).
        public static object? ConvertValue(string? raw, Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var isNullable = underlying != targetType || underlying == typeof(string);

            if (string.IsNullOrEmpty(raw))
            {
                if (isNullable)
                {
                    return null;
                }
                throw new ArgumentException("قيمة الحقل مطلوبة.");
            }

            try
            {
                if (underlying == typeof(string)) return raw;
                if (underlying == typeof(bool)) return bool.Parse(raw);
                if (underlying == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
                if (underlying == typeof(decimal)) return decimal.Parse(raw, NumberStyles.Number, CultureInfo.InvariantCulture);
                if (underlying == typeof(DateTime)) return DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                throw new ArgumentException("قيمة غير صالحة لنوع هذا الحقل.");
            }

            throw new NotSupportedException($"نوع حقل غير مدعوم: {targetType.Name}");
        }
    }
}
