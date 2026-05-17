using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.DTOs
{
    public class DashboardInsightsDTO
    {
        public InsightItemDTO Projects { get; set; } = new();
        public InsightItemDTO Tasks { get; set; } = new();
        public InsightItemDTO PullRequests { get; set; } = new();
        public InsightItemDTO Members { get; set; } = new();
    }
    public class InsightItemDTO
    {
        public int Total { get; set; }
        public List<InsightBreakdownDTO> Breakdown { get; set; } = new();
    }
    public class InsightBreakdownDTO
    {
        public string Key { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }

    }
    public enum TimeRange { Day, Week, Month }

    public class UserWorkloadDTO
    {
        public string FullName { get; set; }
        public int TaskCount { get; set; }
        public double LoadPercentage { get; set; }
    }

    public class WorkloadInsightsDTO
    {
        public List<UserWorkloadDTO> PeakDistribution { get; set; }
        public List<UserWorkloadDTO> AvailableBandwidth { get; set; }
    }
    public class UserPRSuccessDTO
    {
        public string FullName { get; set; }
        public double ApprovedCount { get; set; }

    }
    public class PREngagementDTO
    {
        public int TotalPRs { get; set; }
        public int TotalApproved { get; set; }
        public int TotalCommented { get; set; }
        public int TotalRequestChanges { get; set; }
        public double SuccessRate { get; set; }
        public List<UserPRSuccessDTO> TopPerformers { get; set; }
    }
}
