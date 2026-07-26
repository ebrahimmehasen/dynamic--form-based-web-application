using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace StudentRegistry.Infrastructure.Export
{
    // Fills the C-column/E-column cells of Templates/StudentExportTemplate.xlsx (never edits the
    // template itself — always opens it fresh and saves a new in-memory copy per student). Cell
    // targets match the registrar's fixed layout: see the mergeCells in the template's sheet1.xml
    // (C5:E5, C6:E6, C8:E8, C12:E12, C13:E13, C14:E14, C15:E15, C18:E18, C19:E19, C20:E20, C21:E21)
    // plus the unmerged pair-cells C7/E7, C9/E9, C10/E10, C11/E11, C22/E22. Verify cell targets
    // against the row LABELS in the template (column B/D), not just cell position — the template
    // has been edited more than once and rows have shifted meaning (e.g. C14/C15 school/year, and
    // the guardian phone row splitting from two rows into one C22/E22 pair).
    //
    // IMPORTANT: this writes cells purely via the raw OpenXML SDK, never via ClosedXML. Verified
    // empirically that loading this specific template into ClosedXML and calling SaveAs() — even
    // with zero picture changes, just one cell edit — corrupts the template's faint background
    // watermark logo when the file is later rendered (it comes out gigantic and fully opaque
    // instead of a small faint mark). Plain OpenXML SDK cell edits leave the drawing part
    // completely untouched, so the watermark renders correctly.
    public class StudentExcelExportService : IStudentExcelExportService
    {
        // The template's merged "الصورة الشخصية" cell is E2:E4. Column E is 31.44140625 chars wide
        // and rows 2-4 are 67.8+46.8+73.8=188.4pt tall; converted to EMU (col via the standard
        // Excel width->pixel formula at 96dpi, row via pt*12700) these are the anchor's real
        // dimensions — required so the inserted picture's shape actually renders (LibreOffice does
        // not derive display size purely from the two-cell anchor markers for a fresh picture; an
        // Extents value that doesn't match the anchor's real box left the picture invisible).
        private const int PhotoBoxWidthEmu = 2_095_500;
        private const int PhotoBoxHeightEmu = 2_392_680;

        private readonly IStudentService _studentService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public StudentExcelExportService(IStudentService studentService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _studentService = studentService;
            _env = env;
            _configuration = configuration;
        }

        public async Task<byte[]> ExportStudentAsync(int studentId)
        {
            var (bytes, _) = await BuildXlsxAsync(studentId);
            return bytes;
        }

        public async Task<byte[]> ExportStudentPdfAsync(int studentId)
        {
            var (xlsxBytes, nationalId) = await BuildXlsxAsync(studentId);
            return ConvertXlsxToPdf(xlsxBytes, nationalId);
        }

        private async Task<(byte[] Bytes, string NationalId)> BuildXlsxAsync(int studentId)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId)
                ?? throw new ArgumentException($"لا يوجد طالب بالمعرف {studentId}.");

            string templatePath = Path.Combine(_env.ContentRootPath, "Templates", "StudentExportTemplate.xlsx");

            using var packageStream = new MemoryStream();
            using (var templateFileStream = File.OpenRead(templatePath))
            {
                await templateFileStream.CopyToAsync(packageStream);
            }
            packageStream.Position = 0;

            using (var doc = SpreadsheetDocument.Open(packageStream, true))
            {
                var wsPart = doc.WorkbookPart!.WorksheetParts.First();

                FillStudentSection(wsPart, student);
                FillGuardianSection(wsPart, student);
                InsertPhoto(wsPart, student);

                wsPart.Worksheet.Save();
                doc.Save();
            }

            return (packageStream.ToArray(), student.NationalId);
        }

        // Converts the filled workbook to PDF via a headless LibreOffice process — this renders
        // the exact template (merges, styles, the photo) rather than reimplementing the layout,
        // so the PDF matches what the registrar sees in Excel. Requires LibreOffice installed on
        // the host; the executable path can be overridden via configuration key "LibreOffice:ExecutablePath".
        private byte[] ConvertXlsxToPdf(byte[] xlsxBytes, string nationalId)
        {
            string sofficePath = ResolveSofficePath();

            string workDir = Path.Combine(Path.GetTempPath(), "student-export-" + Guid.NewGuid().ToString("N"));
            string profileDir = Path.Combine(workDir, "profile");
            Directory.CreateDirectory(workDir);
            Directory.CreateDirectory(profileDir);

            try
            {
                string inputPath = Path.Combine(workDir, $"{nationalId}.xlsx");
                File.WriteAllBytes(inputPath, xlsxBytes);

                var psi = new ProcessStartInfo
                {
                    FileName = sofficePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("--headless");
                psi.ArgumentList.Add("--norestore");
                psi.ArgumentList.Add("-env:UserInstallation=file:///" + profileDir.Replace('\\', '/'));
                psi.ArgumentList.Add("--convert-to");
                psi.ArgumentList.Add("pdf");
                psi.ArgumentList.Add("--outdir");
                psi.ArgumentList.Add(workDir);
                psi.ArgumentList.Add(inputPath);

                using var process = Process.Start(psi)
                    ?? throw new InvalidOperationException("تعذر تشغيل LibreOffice لتحويل الملف إلى PDF.");

                string stderr = process.StandardError.ReadToEnd();
                bool exited = process.WaitForExit(60_000);
                if (!exited)
                {
                    process.Kill(true);
                    throw new TimeoutException("انتهت المهلة أثناء تحويل الملف إلى PDF.");
                }
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"فشل تحويل الملف إلى PDF (كود الخروج {process.ExitCode}): {stderr}");
                }

                string outputPath = Path.Combine(workDir, $"{nationalId}.pdf");
                if (!File.Exists(outputPath))
                    throw new InvalidOperationException("لم يتم إنشاء ملف PDF.");

                return File.ReadAllBytes(outputPath);
            }
            finally
            {
                try { Directory.Delete(workDir, true); } catch { /* best-effort cleanup */ }
            }
        }

        private string ResolveSofficePath()
        {
            string? configured = _configuration["LibreOffice:ExecutablePath"];
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
                return configured;

            string[] commonWindowsPaths =
            {
                @"C:\Program Files\LibreOffice\program\soffice.exe",
                @"C:\Program Files (x86)\LibreOffice\program\soffice.exe"
            };
            foreach (var path in commonWindowsPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // Fall back to relying on PATH (typical on Linux hosts: "soffice").
            return "soffice";
        }

        // Adds the student's photo as a brand-new picture anchored at merged cell E2:E4 — this is
        // an empty bordered box in the template with no existing picture in it (the only picture
        // anywhere near it, "Picture 2", is a faint full-page background watermark logo spanning
        // B5:E21 and must never be touched).
        private void InsertPhoto(WorksheetPart wsPart, StudentResponseDto s)
        {
            if (string.IsNullOrWhiteSpace(s.PhotoPath))
                return;

            string absolutePhotoPath = Path.Combine(_env.WebRootPath, s.PhotoPath);
            if (!File.Exists(absolutePhotoPath))
                return;

            string contentType = Path.GetExtension(absolutePhotoPath).TrimStart('.').ToLowerInvariant() switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "webp" => "image/webp",
                _ => "image/png"
            };

            var drawingsPart = wsPart.DrawingsPart;
            if (drawingsPart == null)
                return;

            const string relationshipId = "rIdStudentPhoto";
            var imagePart = drawingsPart.AddNewPart<ImagePart>(contentType, relationshipId);
            using (var photoStream = File.OpenRead(absolutePhotoPath))
            {
                imagePart.FeedData(photoStream);
            }

            string anchorXml = $@"<xdr:twoCellAnchor xmlns:xdr=""http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing"" xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" editAs=""oneCell"">
<xdr:from><xdr:col>4</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>1</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:from>
<xdr:to><xdr:col>5</xdr:col><xdr:colOff>0</xdr:colOff><xdr:row>4</xdr:row><xdr:rowOff>0</xdr:rowOff></xdr:to>
<xdr:pic>
<xdr:nvPicPr>
<xdr:cNvPr id=""1000"" name=""StudentPhoto""/>
<xdr:cNvPicPr><a:picLocks noChangeAspect=""1""/></xdr:cNvPicPr>
</xdr:nvPicPr>
<xdr:blipFill>
<a:blip r:embed=""{relationshipId}""/>
<a:stretch><a:fillRect/></a:stretch>
</xdr:blipFill>
<xdr:spPr bwMode=""auto"">
<a:xfrm><a:off x=""0"" y=""0""/><a:ext cx=""{PhotoBoxWidthEmu}"" cy=""{PhotoBoxHeightEmu}""/></a:xfrm>
<a:prstGeom prst=""rect""><a:avLst/></a:prstGeom>
<a:noFill/>
<a:ln><a:noFill/></a:ln>
</xdr:spPr>
</xdr:pic>
<xdr:clientData/>
</xdr:twoCellAnchor>";

            var anchor = new Xdr.TwoCellAnchor(anchorXml);
            drawingsPart.WorksheetDrawing.Append(anchor);
            drawingsPart.WorksheetDrawing.Save();
        }

        private static void SetCellValue(WorksheetPart wsPart, string cellReference, string value)
        {
            var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>()!;
            uint rowIndex = uint.Parse(new string(cellReference.Where(char.IsDigit).ToArray()));
            var row = sheetData.Elements<Row>().First(r => r.RowIndex! == rowIndex);
            var cell = row.Elements<Cell>().First(c => c.CellReference! == cellReference);
            cell.RemoveAllChildren();
            cell.DataType = CellValues.InlineString;
            // Every target cell's style already has readingOrder="2" (RTL), but LibreOffice's
            // renderer still left-aligns a run when its content starts with digits (national IDs,
            // phone numbers) because it re-derives paragraph direction from the first strong
            // character. Prefixing with U+200F (right-to-left mark, invisible) forces RTL regardless
            // of what the value starts with.
            cell.AppendChild(new InlineString(new Text(RightToLeftMark + value) { Space = SpaceProcessingModeValues.Preserve }));
        }

        // U+200F RIGHT-TO-LEFT MARK — invisible, forces RTL reading order for the run regardless
        // of what character the value starts with (see SetCellValue).
        private const string RightToLeftMark = "‏";

        private static void FillStudentSection(WorksheetPart wsPart, StudentResponseDto s)
        {
            string fullAddress = FormatAddress(s.AddressGov, s.AddressCenter, s.AddressVillage, s.AddressStreet, s.AddressBuilding, s.AddressFloor);
            string birthLocation = string.Join(" - ", new[] { s.BirthCountry, s.BirthGovernorate, s.BirthCity }.Where(v => !string.IsNullOrWhiteSpace(v)));

            SetCellValue(wsPart, "C5", s.StudentName);
            SetCellValue(wsPart, "C6", s.NationalId);
            // Household landline — the form collects one shared landline (stored as GuardianLandlinePhone)
            // and it appears in both the student block (C7) and the guardian block (C22).
            SetCellValue(wsPart, "C7", s.GuardianLandlinePhone ?? string.Empty);
            SetCellValue(wsPart, "E7", s.Phone);
            SetCellValue(wsPart, "C8", fullAddress);
            // Row 9 ("محل الميلاد") is now a single merged C9:E9 cell — AddressCenter/AddressGov no
            // longer have their own row at all (AddressGov is still folded into C8's fullAddress).
            SetCellValue(wsPart, "C9", birthLocation);
            SetCellValue(wsPart, "C10", s.Gender);
            // Row 11's second field (previously BirthGovernorate at E11) lost its label entirely —
            // there's nowhere left to show it.
            SetCellValue(wsPart, "C11", s.BirthDate.ToString("dd/MM/yyyy"));
            SetCellValue(wsPart, "C12", ResolveScoreCell(s));
            SetCellValue(wsPart, "C13", s.Certification);
            // Template row labels: C14 = "المدرسة الحاصل منها على الثانوية العامة" (school),
            // C15 = "سنة الحصول على الثانوية العامة أو ما يعادلها" (graduation year) — not the
            // other way around.
            SetCellValue(wsPart, "C14", s.SchoolName);
            SetCellValue(wsPart, "C15", s.GraduationYear.ToString());
        }

        private static void FillGuardianSection(WorksheetPart wsPart, StudentResponseDto s)
        {
            string fullAddress = FormatAddress(s.AddressGov, s.AddressCenter, s.AddressVillage, s.AddressStreet, s.AddressBuilding, s.AddressFloor);

            SetCellValue(wsPart, "C18", s.GuardianName);
            SetCellValue(wsPart, "C19", s.GuardianOccupation);
            SetCellValue(wsPart, "C20", s.GuardianNationalId);
            // No separate guardian address is collected — the guardian lives with the student,
            // so this reuses the same formatted address as C8.
            SetCellValue(wsPart, "C21", fullAddress);
            // Row 22 is back down to a single "رقم الهاتف" field (its old two-label landline/mobile
            // split is gone) — only C22 has a label now, so only the guardian's phone goes there;
            // there's no separate slot for the guardian's landline anymore (still shown once, for
            // the household, at C7).
            SetCellValue(wsPart, "C22", s.GuardianPhone);
        }

        private static string FormatAddress(string gov, string center, string? village, string street, string building, string? floor)
        {
            var parts = new[] { gov, center, village, street, building, floor }
                .Where(v => !string.IsNullOrWhiteSpace(v));
            return string.Join(" - ", parts);
        }

        // "المجموع الاعتباري (المجموع المصري)" cell: for the Egyptian certificate itself, this is
        // the raw achieved total (FinalTotal); for every equivalent/foreign certificate, this is
        // the percentage instead (per registrar's instruction — not the converted EquivalentTotal).
        private static string ResolveScoreCell(StudentResponseDto s)
        {
            if (s.EgyptianTotals != null)
                return s.EgyptianTotals.FinalTotal.ToString("0.##");
            if (s.SaudiTotals != null)
                return s.SaudiTotals.FinalPercentage.ToString("0.##");
            if (s.KuwaitiTotals != null)
                return s.KuwaitiTotals.FinalPercentage.ToString("0.##");
            if (s.QatariTotals != null)
                return s.QatariTotals.Percentage.ToString("0.##");
            if (s.OmaniTotals != null)
                return s.OmaniTotals.Percentage.ToString("0.##");
            if (s.YemeniTotals != null)
                return s.YemeniTotals.Percentage.ToString("0.##");
            if (s.BahrainiTotals != null)
                return s.BahrainiTotals.Percentage.ToString("0.##");
            if (s.PalestinianTotals != null)
                return s.PalestinianTotals.Percentage.ToString("0.##");
            if (s.AzharTotals != null)
                return s.AzharTotals.Percentage.ToString("0.##");
            if (s.EmiratiTotals != null)
                return s.EmiratiTotals.Percentage.ToString("0.##");
            if (s.OtherTotals != null)
                return s.OtherTotals.Percentage.ToString("0.##");
            if (s.AmericanDiplomaTotals != null)
                return s.AmericanDiplomaTotals.BasePercentage.ToString("0.##");
            if (s.IgGrades != null)
                return s.IgGrades.ScorePercentage.ToString("0.##");
            if (s.StandardGrades != null && s.StandardGrades.Any())
                return s.StandardGrades.Average(g => g.WeightedPercentage).ToString("0.##");

            return string.Empty;
        }
    }
}
