using TA_Application.DTOs;
using TA_Domain.Entities;
using TA_Domain.Enums;

namespace TA_Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<PagedResult<TeamDTO>> GetAllTeamsAsync(int companyId, CancellationToken ct);
        Task<List<TeamDTO>> GetAllTeams(int companyId,string userId, string role, CancellationToken ct);
        Task<TeamMembersDTO> GetUsersOfTeams(int companyId, CancellationToken ct);
        Task<MemberTeamsDTO> GetUserTeams(string userId, CancellationToken ct);
        Task<TeamDTO> GetByIdAsync(int teamId, CancellationToken ct);
        Task<ResponseDTO> SaveTeamAsync(TeamDTO dto, CancellationToken ct);
        Task<ResponseDTO> UpdateMemberStatusAsync(string userId, int teamId, MembershipStatus status, CancellationToken ct);
        Task<ResponseDTO> UpdateTeamAsync(int teamId, TeamDTO dto, CancellationToken ct);
        Task<ResponseDTO> DeleteTeamAsync(int teamId, string deletedByUser, CancellationToken ct);
        Task<ResponseDTO> SendTeamInvitationsAsync(List<string> userIds, List<int> teamIds, string userId, CancellationToken ct);
        Task<ResponseDTO> AddUserToTeamsAsync(AssignMemberToTeamsDTO dto, CancellationToken ct);
        Task<ResponseDTO> RemoveUserFromTeamAsync(string userId, int teamId, CancellationToken ct);
        Task<ResponseDTO> AssignProjectWithTeamsAsync(int projectId, List<int> teamIds, string createdByUser, CancellationToken ct);
        Task<ResponseDTO> UnassignProjectFromTeamAsync(int projectId, int teamId, string updatedByUser, CancellationToken ct);
        Task<List<ProjectTeamDTO>> GetProjectTeamsAsync(int projectId, CancellationToken ct);
        Task<List<TeamDTO>> GetAvailableTeamsForProjectAsync(int companyId, int projectId, CancellationToken ct);
    }
}
