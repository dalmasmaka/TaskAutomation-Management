using TA_Domain.Entities;
using TA_Domain.Enums;

namespace TA_Application.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetAllTeamsAsync(int companyId, CancellationToken ct);
        Task<List<TeamMember>> GetAllUserTeams(string userId, CancellationToken ct);
        Task<Team> GetByIdAsync(int teamId, CancellationToken ct);
        Task<Team> SaveAsync(Team team, CancellationToken ct);
        Task<List<TeamMember>> GetAllTeamMembersAsync(int companyId, CancellationToken ct);
        Task<List<(TeamMember Member, string RoleName)>> GetTeamMembersAsync(int companyId, int? teamId, CancellationToken ct);
        Task<TeamMember> UpdateMemberStatusAsync(string userId, int teamId, MembershipStatus status, CancellationToken ct);
        Task<bool> DeleteAsync(int teamId, string deletedByUser, CancellationToken ct);
        Task<bool> AddUserToTeamsAsync(string userId, List<int> teamIds, string createdByUser, CancellationToken ct);
        Task<bool> SendTeamInvitationsAsync(List<string> userIds, List<int> teamIds, string userId, CancellationToken ct);
        Task<bool> AddProjectToTeamsAsync(int projectId, List<int> teamIds, string createdByUser, CancellationToken ct);
        Task<bool> RemoveUserFromTeamAsync(string userId, int teamId, CancellationToken ct);
        Task<bool> UnassignProjectFromTeamAsync(int projectId, int teamId, string updatedByUser, CancellationToken ct);
        Task<List<ProjectTeam>> GetProjectTeamsAsync(int projectId, CancellationToken ct);
        Task<List<ProjectTeam>> GetProjectsOfTeamAsync(int teamId, CancellationToken ct);
    }
}
