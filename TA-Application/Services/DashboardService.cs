using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;

namespace TA_Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IPullRequestRepository _pullRequestRepository;
        public DashboardService(ILogger<DashboardService> logger, ICompanyRepository companyRepository, IProjectRepository projectRepository, IPullRequestRepository pullRequestRepository, ITasksRepository tasksRepository)
        {
            _logger = logger;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _pullRequestRepository = pullRequestRepository;
            _tasksRepository = tasksRepository;
        }
        private static InsightItemDTO BuildInsight<T>(IEnumerable<T> source, Func<T, string> groupSelector)
        {
            var items = source.ToList();

            var total = items.Count;

            return new InsightItemDTO
            {
                Total = total,

                Breakdown = items
                    .GroupBy(groupSelector)
                    .Select(g => new InsightBreakdownDTO
                    {
                        Key = g.Key,
                        Count = g.Count(),

                        Percentage = total == 0
                            ? 0
                            : Math.Round(
                                (double)g.Count() / total * 100,
                                2)
                    })
                    .ToList()
            };
        }
        public async Task<DashboardInsightsDTO> GetDashboardInsightsAsync(string userID, string role, int companyID)
        {
            if (string.IsNullOrWhiteSpace(userID))
                throw new ArgumentNullException(nameof(userID));

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentNullException(nameof(role));

            if (companyID <= 0)
                throw new ArgumentNullException(nameof(companyID));

            try
            {
                var team = await _companyRepository.GetTeamOfCompany(companyID);

                var projects = await _projectRepository.GetAllAsync(companyID, userID);

                var tasks = _tasksRepository.GetAllTasks(userID, role, companyID);

                var prs = _pullRequestRepository.GetAllPRsAsync(userID, role, companyID);

                return new DashboardInsightsDTO
                {
                    Projects = BuildInsight(
                        projects,
                        p => p.IsAssigned.ToString()
                    ),

                    Tasks = BuildInsight(
                        tasks,
                        t => t.Status.ToString()
                    ),

                    PullRequests = BuildInsight(
                        prs,
                        pr => pr.Status.ToString()
                    ),

                    Members = BuildInsight(
                        team,
                        m => m.Role
                    )
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard insights!");
                throw;
            }
        }

        public async Task<PREngagementDTO> GetPREngagementAsync(
           int companyId,
           string userId,
           string userRole,
           TimeRange range)
        {
            DateTime startDate = range switch
            {
                TimeRange.Day => DateTime.UtcNow.AddDays(-1),
                TimeRange.Week => DateTime.UtcNow.AddDays(-7),
                TimeRange.Month => DateTime.UtcNow.AddMonths(-1),
                _ => DateTime.UtcNow.AddDays(-7)
            };

            var workspaceData = await _pullRequestRepository
                .GetWorkspaceReviews(companyId, startDate);

            if (!workspaceData.Any())
                return new PREngagementDTO();

            var statsData = userRole == "Developer"
                ? workspaceData.Where(x => x.DeveloperId == userId).ToList()
                : workspaceData;

            int total = statsData.Count;

            var decisionStats = statsData
                .GroupBy(x => x.Decision)
                .ToDictionary(g => g.Key, g => g.Count());

            int approved = decisionStats.GetValueOrDefault("Approved");

            return new PREngagementDTO
            {
                TotalPRs = total,
                TotalApproved = approved,
                TotalCommented = decisionStats.GetValueOrDefault("Commented"),
                TotalRequestChanges = decisionStats.GetValueOrDefault("Changes_Requested"),

                SuccessRate = total == 0
                    ? 0
                    : Math.Round((double)approved / total * 100, 1),

                TopPerformers = workspaceData
                    .Where(x => x.Decision == "Approved")
                    .GroupBy(x => x.Developer)
                    .Select(g => new UserPRSuccessDTO
                    {
                        FullName = g.Key,
                        ApprovedCount = g.Count()
                    })
                    .OrderByDescending(x => x.ApprovedCount)
                    .Take(5)
                    .ToList()
            };
        }

        public async Task<WorkloadInsightsDTO> GetWorkloadInsightsAsync(TimeRange range, int companyId)
        {
            DateTime startDate = range switch
            {
                TimeRange.Day => DateTime.UtcNow.AddDays(-1),
                TimeRange.Week => DateTime.UtcNow.AddDays(-7),
                TimeRange.Month => DateTime.UtcNow.AddMonths(-1),
                _ => DateTime.UtcNow.AddDays(-7)
            };

            var userTasksList = await _tasksRepository.GetCompanyMembersTasks(companyId, startDate);

            int totalTasksInRange = userTasksList.Sum(x => x.TaskCount);
            foreach (var stat in userTasksList)
            {
                double rawPercentage = totalTasksInRange > 0
              ? (double)stat.TaskCount / totalTasksInRange * 100
              : 0;

                stat.LoadPercentage = Math.Round(rawPercentage, 1);
            }
            // Calculate the mathematical average load per person
            double averageLoad = userTasksList.Average(x => x.LoadPercentage);

            return new WorkloadInsightsDTO
            {
                // Users doing more than the average "fair share"
                PeakDistribution = userTasksList
                    .Where(x => x.LoadPercentage > averageLoad && x.TaskCount > 0)
                    .OrderByDescending(x => x.LoadPercentage)
                      .Take(5)
                    .ToList(),

                // Users doing less than the average (or nothing at all)
                AvailableBandwidth = userTasksList
                    .Where(x => x.LoadPercentage <= averageLoad)
                    .OrderBy(x => x.LoadPercentage)
                    .Take(5)
                    .ToList()
            };
        }
    }
}
