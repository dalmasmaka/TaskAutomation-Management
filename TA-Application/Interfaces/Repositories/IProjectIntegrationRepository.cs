using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface IProjectIntegrationRepository
    {
        Task<ProjectIntegration?> GetByProjectIdAsync(int projectId, CancellationToken ct);
        Task<ProjectIntegration> SaveAsync(ProjectIntegration integration, CancellationToken ct);
        Task<List<TA_Domain.Entities.Task>> SaveTasksAsync(List<TA_Domain.Entities.Task> tasks, CancellationToken ct);
        Task<List<ProjectIntegration>> GetExpiringJiraTokens(CancellationToken ct);
        Task<ProjectIntegration> GetProjectByExternalId(string externalId, CancellationToken ct);
    }
}
