using Microsoft.Extensions.Logging;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Enums;

namespace TA_Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<TeamService> _logger;
        public TeamService(ITeamRepository teamRepository, ILogger<TeamService> logger)
        {
            _teamRepository = teamRepository;
            _logger = logger;
        }
        public async Task<ResponseDTO> AddUserToTeamsAsync(AssignMemberToTeamsDTO dto, CancellationToken ct)
        {
            try
            {
                var result = await _teamRepository.AddUserToTeamsAsync(dto.UserId, dto.TeamIds, dto.ActionByUserId, ct);

                if (!result)
                {
                    _logger.LogWarning("Failed to assign user to teams. UserId: {UserId}, TeamIds: {@TeamIds}", dto.UserId, dto.TeamIds);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "User could not be assigned to teams."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "User assigned to teams successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("AddUsersToTeamsAsync was cancelled. UserId: {UserId}", dto.UserId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user to teams. UserId: {UserId}, TeamIds: {@TeamIds}", dto.UserId, dto.TeamIds);
                throw;
            }
        }

        public async Task<ResponseDTO> DeleteTeamAsync(int teamId, string deletedByUser, CancellationToken ct)
        {
            try
            {
                var team = await _teamRepository.GetByIdAsync(teamId, ct);

                if (team is null)
                {
                    _logger.LogWarning("DeleteTeamAsync: Team not found. TeamId: {TeamId}", teamId);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Team not found."
                    };
                }

                await _teamRepository.DeleteAsync(teamId, deletedByUser, ct);

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Team deleted successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteTeamAsync was cancelled. TeamId: {TeamId}", teamId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team. TeamId: {TeamId}", teamId);
                throw;
            }
        }
        public async Task<PagedResult<TeamDTO>> GetAllTeamsAsync(int companyId, CancellationToken ct)
        {
            try
            {
                var teams = await _teamRepository.GetAllTeamsAsync(companyId, ct);

                if (teams == null || !teams.Any())
                {
                    return new PagedResult<TeamDTO>
                    {
                        Items = new List<TeamDTO>(),
                        TotalCount = 0,

                    };
                }

                var mapped = teams.Select(t => new TeamDTO
                {
                    TeamId = t.Id,
                    TeamName = t.TeamName,
                    CompanyId = t.CompanyId,
                    CreatedAt = t.CreatedAt.ToString("dd MMMM yyyy"),
                }).ToList();

                return new PagedResult<TeamDTO>
                {
                    Items = mapped,
                    TotalCount = mapped.Count,

                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetAllTeamsAsync was cancelled. CompanyId: {CompanyId}", companyId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams. CompanyId: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<TeamDTO>> GetAllTeams(int companyId, string userId, string role, CancellationToken ct)
        {
            try
            {
                var teams = await _teamRepository.GetAllTeamsAsync(companyId, ct);
                if (role == "Developer")
                {
                    var teamsWithUser = (await GetUserTeams(userId, ct)).ActiveTeams.Select(t => t.TeamId).ToHashSet();
                    teams = teams.Where(t => teamsWithUser.Contains(t.Id)).ToList();
                }
                var mapped = teams.Select(t => new TeamDTO
                {
                    TeamId = t.Id,
                    TeamName = t.TeamName,
                    CompanyId = t.CompanyId,
                    CreatedAt = t.CreatedAt.ToString("dd MMMM yyyy"),
                    CreatedByUser = t.CreatedBy.FullName
                }).ToList();
                return mapped;

            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetAllTeamsAsync was cancelled. CompanyId: {CompanyId}", companyId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams. CompanyId: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<TeamDTO>> GetAvailableTeamsForProjectAsync(int companyId, int projectId, CancellationToken ct)
        {
            try
            {
                var allTeams = await GetAllTeamsAsync(companyId, ct);

                var assignedTeams = await GetProjectTeamsAsync(projectId, ct);

                var assignedTeamIds = assignedTeams
                    .Select(t => t.TeamId)
                    .ToHashSet();

                var unassignedTeams = allTeams.Items
                    .Where(t => !assignedTeamIds.Contains(t.TeamId))
                    .ToList();

                return unassignedTeams;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetProjectTeamsAsync was cancelled for ProjectId: {ProjectId}", projectId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams for ProjectId {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<TeamDTO> GetByIdAsync(int teamId, CancellationToken ct)
        {
            try
            {
                var entity = await _teamRepository.GetByIdAsync(teamId, ct);

                if (entity is null)
                {
                    _logger.LogWarning("Team not found. TeamId: {TeamId}", teamId);
                    throw new KeyNotFoundException("Team not found");
                }

                return new TeamDTO
                {
                    TeamId = entity.Id,
                    TeamName = entity.TeamName,
                    CompanyId = entity.CompanyId
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetByIdAsync was cancelled for TeamId: {TeamId}", teamId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching team by Id {TeamId}", teamId);
                throw;
            }
        }

        public async Task<List<ProjectTeamDTO>> GetProjectTeamsAsync(int projectId, CancellationToken ct)
        {
            try
            {
                var entities = await _teamRepository.GetProjectTeamsAsync(projectId, ct);

                return entities.Select(entity => new ProjectTeamDTO
                {
                    TeamId = entity.TeamId,
                    TeamName = entity.Team.TeamName,
                    ProjectId = entity.ProjectId,
                    AssignedAt = entity.CreatedAt.ToString("dd MMM yyyy")
                }).ToList();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetProjectTeamsAsync was cancelled for ProjectId: {ProjectId}", projectId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams for ProjectId {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<TeamMembersDTO> GetUsersOfTeams(int companyId, CancellationToken ct)
        {
            try
            {
                var entities = await _teamRepository.GetAllTeamMembersAsync(companyId, ct);

                var allMembers = entities.Select(entity => new MemberDTO
                {
                    // Map your properties here, for example:
                    Id = entity.Id,
                    FullName = entity.User.FullName,
                    Email = entity.User.Email,
                    PlainStatus = entity.Status,
                    Status = entity.Status.GetDisplayName(),
                    CreatedAt = entity.CreatedAt.ToString("dd MMM yyy"),
                    UpdatedAt = entity.UpdatedAt.ToString("dd MMM yyy"),
                    InvitedBy = entity.CreatedBy.FullName,
                    TeamName = entity.Team.TeamName,
                    TeamId = entity.TeamId,
                    UserId = entity.UserId
                }).ToList();

                // 2. Return a single DTO, filtering the master list by status
                return new TeamMembersDTO
                {
                    ActiveMembers = allMembers
                        .Where(m => m.PlainStatus == MembershipStatus.Active)
                        .ToList(),

                    PendingMembers = allMembers
                        .Where(m => m.PlainStatus == MembershipStatus.PendingApproval)
                        .ToList(),

                    DeclinedMembers = allMembers
                        .Where(m => m.PlainStatus == MembershipStatus.Declined)
                        .ToList()
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetUsersOfTeams was cancelled for CompanyId: {CompanyId}", companyId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams for CompanyId {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<ResponseDTO> UnassignProjectFromTeamAsync(int projectId, int teamId, string updatedByUser, CancellationToken ct)
        {
            try
            {
                var result = await _teamRepository.UnassignProjectFromTeamAsync(
                    projectId,
                    teamId,
                    updatedByUser,
                    ct);

                if (!result)
                {
                    _logger.LogWarning(
                        "Failed to remove project from team. ProjectId: {ProjectId}, TeamId: {TeamId}",
                        projectId, teamId);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Project could not be removed from team."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Project removed from team successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "UnassignProjectFromTeamAsync was cancelled. ProjectId: {ProjectId}, TeamId: {TeamId}",
                    projectId, teamId);

                return new ResponseDTO
                {
                    Success = false,
                    Message = "Operation was cancelled."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error removing project from team. ProjectId: {ProjectId}, TeamId: {TeamId}",
                    projectId, teamId);

                return new ResponseDTO
                {
                    Success = false,
                    Message = "An unexpected error occurred while removing project from team."
                };
            }
        }

        public async Task<ResponseDTO> RemoveUserFromTeamAsync(string userId, int teamId, CancellationToken ct)
        {
            try
            {
                var result = await _teamRepository.RemoveUserFromTeamAsync(userId, teamId, ct);

                if (!result)
                {
                    _logger.LogWarning("Failed to remove user from team. UserId: {UserId}, TeamId: {TeamId}", userId, teamId);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "User could not be removed from team."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Revoked access from member successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("RemoveUserFromTeamAsync was cancelled. UserId: {UserId}, TeamId: {TeamId}", userId, teamId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from team. UserId: {UserId}, TeamId: {TeamId}", userId, teamId);
                throw;
            }
        }

        public async Task<ResponseDTO> SaveTeamAsync(TeamDTO dto, CancellationToken ct)
        {
            try
            {
                var entity = new TA_Domain.Entities.Team
                {
                    Id = dto.TeamId,
                    TeamName = dto.TeamName,
                    CompanyId = dto.CompanyId,
                };
                if (dto.TeamId == 0)
                {
                    entity.CreatedById = dto.ActionByUserId;
                    entity.CreatedAt = DateTime.Now;
                }
                else
                {
                    entity.UpdatedById = dto.ActionByUserId;
                    entity.UpdatedAt = DateTime.Now;
                }
                var saved = await _teamRepository.SaveAsync(entity, ct);
                if (dto.TeamId != 0 && saved == null)
                {
                    return new ResponseDTO
                    {
                        Success = true,
                        Message = $"Team with Id {dto.TeamId} was not found."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Team updated successfully."
                };

            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SaveTeamAsync was cancelled. TeamName: {TeamName}", dto.TeamName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving team. TeamName: {TeamName}", dto.TeamName);
                throw;
            }
        }

        public async Task<ResponseDTO> UpdateMemberStatusAsync(
            string userId,
            int teamId,
            MembershipStatus status,
            CancellationToken ct)
        {
            try
            {
                var entity = await _teamRepository.UpdateMemberStatusAsync(userId, teamId, status, ct);

                if (entity == null)
                {
                    throw new KeyNotFoundException(
                        $"Member with UserId '{userId}' in TeamId '{teamId}' was not found.");
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Member status updated successfully",
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "UpdateMemberStatusAsync was cancelled. UserId: {UserId}, TeamId: {TeamId}, Status: {Status}",
                    userId,
                    teamId,
                    status);

                throw;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Member not found while updating status. UserId: {UserId}, TeamId: {TeamId}",
                    userId,
                    teamId);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating member status. UserId: {UserId}, TeamId: {TeamId}, Status: {Status}",
                    userId,
                    teamId,
                    status);

                throw;
            }
        }

        public async Task<ResponseDTO> UpdateTeamAsync(int teamId, TeamDTO dto, CancellationToken ct)
        {
            try
            {
                var entity = await _teamRepository.GetByIdAsync(teamId, ct);

                if (entity is null)
                {
                    _logger.LogWarning("Update failed. Team not found. TeamId: {TeamId}", teamId);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Team not found."
                    };
                }

                entity.TeamName = dto.TeamName;
                //entity.CompanyId = dto.CompanyId;

                await _teamRepository.SaveAsync(entity, ct);

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Team updated successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("UpdateTeamAsync was cancelled. TeamId: {TeamId}", teamId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team. TeamId: {TeamId}", teamId);
                throw;
            }
        }

        public async Task<ResponseDTO> AssignProjectWithTeamsAsync(int projectId, List<int> teamIds, string createdByUser, CancellationToken ct)
        {
            try
            {
                var result = await _teamRepository.AddProjectToTeamsAsync(projectId, teamIds, createdByUser, ct);

                if (!result)
                {
                    _logger.LogWarning(
                        "Failed to assign project to teams. ProjectId: {ProjectId}, TeamIds: {TeamIds}",
                        projectId, string.Join(",", teamIds));

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "One or more teams could not be found or assigned."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Project assigned to teams successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "AssignProjectToTeamsAsync was cancelled. ProjectId: {ProjectId}, TeamIds: {TeamIds}",
                    projectId, string.Join(",", teamIds));

                return new ResponseDTO
                {
                    Success = false,
                    Message = "Operation was cancelled."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error assigning project to teams. ProjectId: {ProjectId}, TeamIds: {TeamIds}",
                    projectId, string.Join(",", teamIds));

                return new ResponseDTO
                {
                    Success = false,
                    Message = "An unexpected error occurred while assigning project to teams."
                };
            }
        }

        public async Task<ResponseDTO> SendTeamInvitationsAsync(List<string> userIds, List<int> teamIds, string userId, CancellationToken ct)
        {
            try
            {
                var result = await _teamRepository.SendTeamInvitationsAsync(userIds, teamIds, userId, ct);

                if (!result)
                {
                    _logger.LogWarning("Failed to send team invitations. SenderId: {SenderId}, TargetUserIds: {@UserIds}, TeamIds: {@TeamIds}", userId, userIds, teamIds);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Team invitations could not be sent to users."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Team invitations sent successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SendTeamInvitationsAsync was cancelled. SenderId: {SenderId}", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending team invitations. SenderId: {SenderId}, TargetUserIds: {@UserIds}, TeamIds: {@TeamIds}", userId, userIds, teamIds);
                throw;
            }
        }

        public async Task<MemberTeamsDTO> GetUserTeams(string userId, CancellationToken ct)
        {
            try
            {
                var entities = await _teamRepository.GetAllUserTeams(userId, ct);

                var allTeams = entities.Select(entity => new MemberDTO
                {
                    Id = entity.Id,
                    FullName = entity.User.FullName,
                    Email = entity.User.Email,
                    PlainStatus = entity.Status,
                    Status = entity.Status.GetDisplayName(),
                    CreatedAt = entity.CreatedAt.ToString("dd MMM yyyy"),
                    UpdatedAt = entity.UpdatedAt.ToString("dd MMM yyyy"),
                    InvitedBy = entity.CreatedBy.FullName,
                    TeamName = entity.Team.TeamName,
                    TeamId = entity.TeamId,
                    UserId = entity.UserId
                }).ToList();

                return new MemberTeamsDTO
                {
                    ActiveTeams = allTeams
                        .Where(m => m.PlainStatus == MembershipStatus.Active)
                        .ToList(),

                    PendingInvitations = allTeams
                        .Where(m => m.PlainStatus == MembershipStatus.PendingApproval)
                        .ToList(),

                    TeamLogs = allTeams
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "GetUserTeams operation was cancelled. UserId: {UserId}",
                    userId);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while fetching teams for UserId: {UserId}",
                    userId);

                throw;
            }
        }
    }
}
