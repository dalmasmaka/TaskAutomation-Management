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
    }
}
