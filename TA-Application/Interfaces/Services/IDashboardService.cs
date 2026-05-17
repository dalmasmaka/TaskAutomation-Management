using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardInsightsDTO> GetDashboardInsightsAsync(string userID, string role, int companyID);
        Task<WorkloadInsightsDTO> GetWorkloadInsightsAsync(TimeRange range, int companyID);
        Task<PREngagementDTO> GetPREngagementAsync(int companyId, string userId, string userRole, TimeRange range);
    }
}
