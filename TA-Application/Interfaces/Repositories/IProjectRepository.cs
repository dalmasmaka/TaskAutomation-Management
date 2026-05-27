using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        Task<Project> SaveAsync(Project project, CancellationToken ct);
        Task<GitHubConfig> SaveGitHubIntegrationAsync(GitHubConfig project, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
        Task<List<Project>> GetAllAsync(int companyId, string userId, CancellationToken ct);
        Task<List<Project>> GetAllByCompanyIdAsync(int companyId, CancellationToken ct);
        Task<List<Project>> GetProjectsForInsights(int companyId, int? teamId, CancellationToken ct);
        Task<List<Project>> GetProjectsAvailableToAssignToUser(int companyId, string userId, CancellationToken ct);
        Task<List<TeamMember>> GetUsersAvailableToAssignToProject(int companyId, int projectId, CancellationToken ct);
        Task<bool> AssignUserToProjects(string userId, List<int> projectIds, CancellationToken ct);
        Task<bool> AssignUsersToProject(int projectId, List<string> userIds, CancellationToken ct);
        Task<List<(ApplicationUser User, string RoleName)>> GetMembersAsync(int projectId, CancellationToken ct);
        Task<bool> RemoveMemberAsync(string userId, int projectId, CancellationToken ct);
        Task <List<Project>> GetProjectsByUserIdAsync(int companyId, string userId, CancellationToken ct);
        Task<GitHubConfig> GetRepoAsync(int projectId, CancellationToken ct);

    }
}
