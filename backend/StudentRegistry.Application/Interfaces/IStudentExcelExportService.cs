using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IStudentExcelExportService
    {
        /// <summary>
        /// Fills a fresh copy of the student export template with the given student's data
        /// and returns the resulting workbook as a byte array. The template file itself is never modified.
        /// </summary>
        Task<byte[]> ExportStudentAsync(int studentId);

        /// <summary>
        /// Same as <see cref="ExportStudentAsync"/>, but converts the filled workbook to PDF
        /// (via a headless LibreOffice conversion) before returning it.
        /// </summary>
        Task<byte[]> ExportStudentPdfAsync(int studentId);
    }
}
