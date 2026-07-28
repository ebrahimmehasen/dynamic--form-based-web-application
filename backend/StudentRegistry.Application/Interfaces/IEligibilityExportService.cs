using System;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IEligibilityExportService
    {
        /// <summary>
        /// Builds an .xlsx listing every student whose EligibilityStatus matches "Eligible" or
        /// "NotEligible" (subject to the same optional date/certification filters as the dashboard).
        /// </summary>
        Task<byte[]> ExportByEligibilityAsync(string eligibilityStatus, DateTime? startDate, DateTime? endDate, string? certification);

        /// <summary>
        /// Builds an .xlsx listing every student matching the given filters, regardless of
        /// eligibility status.
        /// </summary>
        Task<byte[]> ExportAllAsync(DateTime? startDate, DateTime? endDate, string? certification);
    }
}
