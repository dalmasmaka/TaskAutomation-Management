using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ITeamRepository _teamRepository;
        public DashboardService(ILogger<DashboardService> logger, ICompanyRepository companyRepository, IProjectRepository projectRepository, IPullRequestRepository pullRequestRepository, ITasksRepository tasksRepository, ITeamRepository teamRepository)
        {
            _logger = logger;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _pullRequestRepository = pullRequestRepository;
            _tasksRepository = tasksRepository;
            _teamRepository = teamRepository;
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
        public async Task<DashboardInsightsDTO> GetDashboardInsightsAsync(string userID, string role, int companyID, int? teamId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userID))
                throw new ArgumentNullException(nameof(userID));

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentNullException(nameof(role));

            if (companyID <= 0)
                throw new ArgumentNullException(nameof(companyID));

            try
            {
                var team = await _teamRepository.GetTeamMembersAsync(companyID, teamId, ct);

                var projects = await _projectRepository.GetProjectsForInsights(companyID, teamId, ct);
                var projectIds = projects.Select(p => p.ProjectId).ToList();
                var tasks = await _tasksRepository.GetAllTasksForInsights(projectIds, userID, role, ct); 
                var taskIds = tasks.Select(p => p.TaskId).ToList();
                var prs = await _pullRequestRepository.GetAllPRsForInsights(taskIds, userID, role, ct);

                return new DashboardInsightsDTO
                {
                    Projects = BuildInsight(
                        projects,
                        p => p.UserProjects?.Any(up => up.UserId == userID && !up.IsDeleted) == true ? "Assigned" : "Unassigned"
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
                        m => m.RoleName
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
           TimeRange range,
           int? teamId,
           CancellationToken ct)
        {
            DateTime startDate = range switch
            {
                TimeRange.Day => DateTime.UtcNow.AddDays(-1),
                TimeRange.Week => DateTime.UtcNow.AddDays(-7),
                TimeRange.Month => DateTime.UtcNow.AddMonths(-1),
                _ => DateTime.UtcNow.AddDays(-7)
            };

            var workspaceData = await _pullRequestRepository
                .GetWorkspacePullRequests(companyId, startDate, teamId, ct);
            
            if (!workspaceData.Any())
                return new PREngagementDTO();

            var statsData = userRole == "Developer"
                ? workspaceData.Where(x => x.CreatedById == userId).ToList()
                : workspaceData;

            int total = statsData.Count;

            var decisionStats = statsData
                .GroupBy(x => x.Status)
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
                    .Where(x => x.Status == "Approved")
                    .GroupBy(x => x.CreatedBy.FullName)
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
        public async Task<WorkloadInsightsDTO> GetWorkloadInsightsAsync(TimeRange range, int companyId, int? teamId, CancellationToken ct)
        {
            DateTime startDate = range switch
            {
                TimeRange.Day => DateTime.UtcNow.AddDays(-1),
                TimeRange.Week => DateTime.UtcNow.AddDays(-7),
                TimeRange.Month => DateTime.UtcNow.AddMonths(-1),
                _ => DateTime.UtcNow.AddDays(-7)
            };

            var userTasksList = await _tasksRepository
                .GetCompanyMembersTasks(companyId, startDate, teamId, ct);

            var mapped = userTasksList
                .GroupBy(tm => tm.UserId)
                .Select(g => new UserWorkloadDTO
                {
                    FullName = g.FirstOrDefault()?.User?.FullName ?? "Unknown",
                    TaskCount = g.Count()
                })
                .ToList();

            int totalTasksInRange = mapped.Sum(x => x.TaskCount);

            foreach (var stat in mapped)
            {
                stat.LoadPercentage = totalTasksInRange > 0
                    ? Math.Round((double)stat.TaskCount / totalTasksInRange * 100, 1)
                    : 0;
            }

            double averageLoad = mapped.Count > 0
                ? mapped.Average(x => x.LoadPercentage)
                : 0;

            return new WorkloadInsightsDTO
            {
                PeakDistribution = mapped
                    .Where(x => x.LoadPercentage > averageLoad && x.TaskCount > 0)
                    .OrderByDescending(x => x.LoadPercentage)
                    .Take(5)
                    .ToList(),

                AvailableBandwidth = mapped
                    .Where(x => x.LoadPercentage <= averageLoad)
                    .OrderBy(x => x.LoadPercentage)
                    .Take(5)
                    .ToList()
            };
        }
    }
}
