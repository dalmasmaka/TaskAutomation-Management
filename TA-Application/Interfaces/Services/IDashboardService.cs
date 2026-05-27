using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardInsightsDTO> GetDashboardInsightsAsync(string userID, string role, int companyID, int? teamId, CancellationToken ct);
        Task<WorkloadInsightsDTO> GetWorkloadInsightsAsync(TimeRange range, int companyID, int? teamId, CancellationToken ct);
        Task<PREngagementDTO> GetPREngagementAsync(int companyId, string userId, string userRole, TimeRange range, int? teamId, CancellationToken ct);
    }
}
