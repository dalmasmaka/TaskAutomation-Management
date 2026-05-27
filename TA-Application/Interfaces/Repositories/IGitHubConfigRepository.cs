using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface IGitHubConfigRepository
    {
        Task<GitHubConfig> GetByProjectId(int projectId, CancellationToken ct);
        Task<UserGitHubConfig> GetUsersGitHubConfig(string UserId, CancellationToken ct);
    }
}
