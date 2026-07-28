using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistry.Infrastructure.Export
{
    // Builds a fresh, plain list-style workbook (unlike StudentExcelExportService, which fills a
    // fixed per-student template) — one row per student. All three dashboard exports (eligible,
    // not-eligible, all) share the exact same comprehensive column set — every scalar field on the
    // Student entity itself (excluding Id/PhotoPath/SubmissionToken, which are internal and not
    // useful in a spreadsheet) — so admins get the full record regardless of which button they use.
    public class EligibilityExportService : IEligibilityExportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EligibilityExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> ExportByEligibilityAsync(string eligibilityStatus, DateTime? startDate, DateTime? endDate, string? certification)
        {
            var students = await _unitOfWork.Dashboard.GetStudentsByEligibilityAsync(eligibilityStatus, startDate, endDate, certification);
            return BuildWorkbook(students);
        }

        public async Task<byte[]> ExportAllAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var students = await _unitOfWork.Dashboard.GetAllStudentsFilteredAsync(startDate, endDate, certification);
            return BuildWorkbook(students);
        }

        private static readonly string[] Headers =
        {
            "الاسم", "الاسم بالإنجليزية", "الرقم القومي", "النوع",
            "تاريخ الميلاد", "دولة الميلاد", "محافظة الميلاد", "مدينة الميلاد",
            "الهاتف", "البريد الإلكتروني",
            "المحافظة", "المركز", "القرية", "الشارع", "المبنى", "الدور",
            "الشهادة", "المسار", "سنة التخرج", "المدرسة",
            "الرغبة (الكلية)", "البرنامج المطلوب", "المجموع الاعتباري (المجموع المصري)", "النسبة المئوية",
            "اسم ولي الأمر", "صلة القرابة", "الرقم القومي لولي الأمر",
            "وظيفة ولي الأمر", "هاتف ولي الأمر", "الهاتف الأرضي",
            "تاريخ التقديم",
            "حالة الاستيفاء", "سبب عدم الاستيفاء", "تم التأكيد بواسطة", "تاريخ التأكيد"
        };

        private static string[] BuildRow(Student s) => new[]
        {
            s.StudentName,
            s.StudentNameEn,
            s.NationalId,
            s.Gender,
            s.BirthDate.ToString("dd/MM/yyyy"),
            s.BirthCountry,
            s.BirthGovernorate,
            s.BirthCity,
            s.Phone,
            s.Email,
            s.AddressGov,
            s.AddressCenter,
            s.AddressVillage ?? string.Empty,
            s.AddressStreet,
            s.AddressBuilding,
            s.AddressFloor ?? string.Empty,
            s.Certification,
            s.Track,
            s.GraduationYear.ToString(),
            s.SchoolName,
            s.WishCollege,
            s.WishProgram ?? string.Empty,
            ResolveEquivalentTotal(s),
            ResolvePercentage(s),
            s.GuardianName,
            s.GuardianRelation,
            s.GuardianNationalId,
            s.GuardianOccupation,
            s.GuardianPhone,
            s.GuardianLandlinePhone ?? string.Empty,
            s.SubmittedAt.ToString("dd/MM/yyyy"),
            EligibilityLabel(s.EligibilityStatus),
            s.EligibilityNote ?? string.Empty,
            s.EligibilityConfirmedBy ?? string.Empty,
            s.EligibilityConfirmedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty
        };

        private static string EligibilityLabel(string? status) => status switch
        {
            "Eligible" => "مستوفي",
            "NotEligible" => "غير مستوفي",
            _ => "لم يتم التأكيد بعد"
        };

        // "المجموع الاعتباري (المجموع المصري)" — same source fields as
        // StudentExcelExportService.ResolveScoreCell's per-student template: for the Egyptian
        // certificate itself this is the raw achieved total (already on the Egyptian scale); for
        // every equivalent/foreign certificate it's the pre-converted EquivalentTotal/GovernmentScore
        // (already computed to the 410-point Egyptian-equivalent scale). Certificates with no
        // equivalent-total concept (Other, American Diploma) are left blank.
        private static string ResolveEquivalentTotal(Student s)
        {
            if (s.EgyptianTotals != null) return s.EgyptianTotals.FinalTotal.ToString("0.##");
            if (s.SaudiTotals != null) return s.SaudiTotals.EquivalentTotal.ToString("0.##");
            if (s.KuwaitiTotals != null) return s.KuwaitiTotals.EquivalentTotal.ToString("0.##");
            if (s.QatariTotals != null) return s.QatariTotals.EquivalentTotal.ToString("0.##");
            if (s.OmaniTotals != null) return s.OmaniTotals.EquivalentTotal.ToString("0.##");
            if (s.YemeniTotals != null) return s.YemeniTotals.EquivalentTotal.ToString("0.##");
            if (s.BahrainiTotals != null) return s.BahrainiTotals.EquivalentTotal.ToString("0.##");
            if (s.PalestinianTotals != null) return s.PalestinianTotals.EquivalentTotal.ToString("0.##");
            if (s.AzharTotals != null) return s.AzharTotals.EquivalentTotal.ToString("0.##");
            if (s.EmiratiTotals != null) return s.EmiratiTotals.EquivalentTotal.ToString("0.##");
            if (s.IgGrades != null) return s.IgGrades.GovernmentScore.ToString("0.##");
            return string.Empty;
        }

        // "النسبة المئوية" — the percentage shown on the site for each certificate type; for the
        // Egyptian certificate this is FinalTotal/Denominator expressed as Percentage, same field
        // used everywhere else in this app.
        private static string ResolvePercentage(Student s)
        {
            if (s.EgyptianTotals != null) return s.EgyptianTotals.Percentage.ToString("0.##") + "%";
            if (s.SaudiTotals != null) return s.SaudiTotals.FinalPercentage.ToString("0.##") + "%";
            if (s.KuwaitiTotals != null) return s.KuwaitiTotals.FinalPercentage.ToString("0.##") + "%";
            if (s.QatariTotals != null) return s.QatariTotals.Percentage.ToString("0.##") + "%";
            if (s.OmaniTotals != null) return s.OmaniTotals.Percentage.ToString("0.##") + "%";
            if (s.YemeniTotals != null) return s.YemeniTotals.Percentage.ToString("0.##") + "%";
            if (s.BahrainiTotals != null) return s.BahrainiTotals.Percentage.ToString("0.##") + "%";
            if (s.PalestinianTotals != null) return s.PalestinianTotals.Percentage.ToString("0.##") + "%";
            if (s.AzharTotals != null) return s.AzharTotals.Percentage.ToString("0.##") + "%";
            if (s.EmiratiTotals != null) return s.EmiratiTotals.Percentage.ToString("0.##") + "%";
            if (s.OtherTotals != null) return s.OtherTotals.Percentage.ToString("0.##") + "%";
            if (s.AmericanDiplomaTotals != null) return s.AmericanDiplomaTotals.EquivalentPercentage.ToString("0.##") + "%";
            if (s.IgGrades != null) return s.IgGrades.ScorePercentage.ToString("0.##") + "%";
            if (s.StandardGrades != null && s.StandardGrades.Any()) return s.StandardGrades.Average(g => g.WeightedPercentage).ToString("0.##") + "%";
            return string.Empty;
        }

        private static byte[] BuildWorkbook(List<Student> students)
        {
            using var stream = new MemoryStream();
            using (var doc = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = doc.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "الطلاب"
                });

                sheetData.Append(BuildXlsxRow(Headers));
                foreach (var s in students)
                {
                    sheetData.Append(BuildXlsxRow(BuildRow(s)));
                }

                worksheetPart.Worksheet.Save();
                workbookPart.Workbook.Save();
            }

            return stream.ToArray();
        }

        private static Row BuildXlsxRow(IEnumerable<string> values)
        {
            var row = new Row();
            foreach (var value in values)
            {
                row.Append(new Cell
                {
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text(value ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve })
                });
            }
            return row;
        }
    }
}
