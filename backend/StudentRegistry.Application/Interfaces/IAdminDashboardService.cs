using StudentRegistry.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync(DateTime? startDate, DateTime? endDate, string? certification);
    }
}
