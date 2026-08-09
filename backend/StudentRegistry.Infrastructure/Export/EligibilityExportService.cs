using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
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
    // Student entity itself (excluding Id/SubmissionToken, which are internal and not useful in a
    // spreadsheet) — so admins get the full record regardless of which button they use. PhotoPath
    // is included, but only as an absolute link in the last column, never the raw relative path.
    public class EligibilityExportService : IEligibilityExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EligibilityExportService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<byte[]> ExportByEligibilityAsync(string eligibilityStatus, DateTime? startDate, DateTime? endDate, string? certification)
        {
            var students = await _unitOfWork.Dashboard.GetStudentsByEligibilityAsync(eligibilityStatus, startDate, endDate, certification);
            return BuildWorkbook(students, GetBaseUrl());
        }

        public async Task<byte[]> ExportAllAsync(DateTime? startDate, DateTime? endDate, string? certification)
        {
            var students = await _unitOfWork.Dashboard.GetAllStudentsFilteredAsync(startDate, endDate, certification);
            return BuildWorkbook(students, GetBaseUrl());
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request != null ? $"{request.Scheme}://{request.Host}" : string.Empty;
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
            "حالة الاستيفاء", "سبب عدم الاستيفاء", "تم التأكيد بواسطة", "تاريخ التأكيد",
            "رابط الصورة الشخصية"
        };

        private static string[] BuildRow(Student s, string baseUrl) => new[]
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
            s.EligibilityConfirmedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
            !string.IsNullOrWhiteSpace(s.PhotoPath) && !string.IsNullOrEmpty(baseUrl) ? $"{baseUrl}/{s.PhotoPath}" : string.Empty
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

        private static byte[] BuildWorkbook(List<Student> students, string baseUrl)
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

                // Photo-link column is the last one; track which rows actually have a photo so a
                // real (clickable) hyperlink relationship can be attached to just those cells.
                string photoColumn = GetColumnLetter(Headers.Length);
                var photoLinks = new List<(string CellReference, string Url)>();
                int rowIndex = 2; // row 1 is the header
                foreach (var s in students)
                {
                    sheetData.Append(BuildXlsxRow(BuildRow(s, baseUrl)));
                    if (!string.IsNullOrWhiteSpace(s.PhotoPath) && !string.IsNullOrEmpty(baseUrl))
                        photoLinks.Add(($"{photoColumn}{rowIndex}", $"{baseUrl}/{s.PhotoPath}"));
                    rowIndex++;
                }

                if (photoLinks.Count > 0)
                {
                    var hyperlinks = new Hyperlinks();
                    foreach (var (cellReference, url) in photoLinks)
                    {
                        var relationshipId = worksheetPart.AddHyperlinkRelationship(new Uri(url, UriKind.Absolute), isExternal: true).Id;
                        hyperlinks.Append(new Hyperlink { Reference = cellReference, Id = relationshipId });
                    }
                    // Schema order requires <hyperlinks> right after <sheetData> here (no
                    // mergeCells/conditionalFormatting/etc. exist on this sheet to come between them).
                    worksheetPart.Worksheet.InsertAfter(hyperlinks, sheetData);
                }

                worksheetPart.Worksheet.Save();
                workbookPart.Workbook.Save();
                // Without this, part relationships added via AddHyperlinkRelationship (the
                // photo-link column) never get flushed to xl/worksheets/_rels/sheet1.xml.rels —
                // the <hyperlink r:id="..."/> in the sheet ends up pointing at nothing.
                doc.Save();
            }

            return stream.ToArray();
        }

        // Converts a 1-based column index to its Excel letter(s) (1 -> A, 27 -> AA, ...).
        private static string GetColumnLetter(int columnIndex)
        {
            string letters = string.Empty;
            while (columnIndex > 0)
            {
                int remainder = (columnIndex - 1) % 26;
                letters = (char)('A' + remainder) + letters;
                columnIndex = (columnIndex - 1) / 26;
            }
            return letters;
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
