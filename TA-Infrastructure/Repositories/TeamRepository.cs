using Microsoft.EntityFrameworkCore;
using System.Data;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Domain.Enums;
using TA_Infrastructure.Persistence;
namespace TA_Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;
        public TeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddProjectToTeamsAsync(int projectId, List<int> teamIds, string createdByUser, CancellationToken ct)
        {
            if (teamIds == null || !teamIds.Any())
                return false;

            var existing = await _context.ProjectTeams
                .Where(x => x.ProjectId == projectId && teamIds.Contains(x.TeamId))
                .ToListAsync(ct);

            
            var existingTeamIds = existing
                .Select(x => x.TeamId)
                .ToHashSet();

            var toRestore = existing
                .Where(x => x.IsDeleted)
                .ToList();

            foreach (var relation in toRestore)
            {
                relation.IsDeleted = false;
            }

            var newRelations = teamIds
                .Where(id => !existingTeamIds.Contains(id))
                .Select(teamId => new ProjectTeam
                {
                    ProjectId = projectId,
                    TeamId = teamId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = createdByUser,
                })
                .ToList();

            if (newRelations.Any())
                await _context.ProjectTeams.AddRangeAsync(newRelations, ct);

            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> AddUserToTeamsAsync(string userId, List<int> teamIds, string createdByUser, CancellationToken ct)
        {
            if (teamIds == null || !teamIds.Any())
                return false;

            var existing = await _context.TeamMembers
                .Where(x => x.UserId == userId && teamIds.Contains(x.TeamId))
                .ToListAsync(ct);

            var existingTeamIds = existing.Select(x => x.TeamId).ToHashSet();

            var newRelations = teamIds
                .Where(id => !existingTeamIds.Contains(id))
                .Select(teamId => new TeamMember
                {
                    UserId = userId,
                    TeamId = teamId
                })
                .ToList();

            if (!newRelations.Any())
                return true;

            await _context.TeamMembers.AddRangeAsync(newRelations, ct);
            await _context.SaveChangesAsync(ct);

            return true;
        }
        public async Task<bool> DeleteAsync(int teamId, string deletedByUser, CancellationToken ct)
        {
            var existing = await _context.Teams
        .FirstOrDefaultAsync(x => x.Id == teamId, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = true;
            existing.UpdatedById = deletedByUser;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Teams.Update(existing);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<List<Team>> GetAllTeamsAsync(int companyId, CancellationToken ct)
        {
            return await _context.Teams
                .Where(t => t.CompanyId == companyId && !t.IsDeleted)
                .Include(t => t.CreatedBy)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .ToListAsync(ct);
        }
        public async Task<List<ProjectTeam>> GetProjectTeamsAsync(int projectId, CancellationToken ct)
        {
            return await _context.ProjectTeams
                .Include(t => t.Team)
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .ToListAsync(ct);
        }
        public async Task<Team> GetByIdAsync(int teamId, CancellationToken ct)
        {
            return await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(t => t.Id == teamId, ct);
        }

        public async Task<bool> UnassignProjectFromTeamAsync(int projectId, int teamId, string updatedByUser, CancellationToken ct)
        {
            var relation = await _context.ProjectTeams.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.TeamId == teamId, ct);

            if (relation == null)
                return false;

            relation.IsDeleted = true;
            relation.UpdatedAt = DateTime.UtcNow;

            _context.ProjectTeams.Update(relation);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> RemoveUserFromTeamAsync(string userId, int teamId, CancellationToken ct)
        {
            var relation = await _context.TeamMembers.FirstOrDefaultAsync(x => x.UserId == userId && x.TeamId == teamId, ct);

            if (relation == null)
                return false;

            relation.IsDeleted = true;
            relation.UpdatedAt = DateTime.UtcNow;

            _context.TeamMembers.Update(relation);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<Team> SaveAsync(Team team, CancellationToken ct)
        {
            if (team.Id == 0)
            {
                await _context.Teams.AddAsync(team, ct);
            }
            else
            {
                var existing = await _context.Teams.FirstOrDefaultAsync(x => x.Id == team.Id, ct);

                if (existing == null)
                    return null;

                existing.TeamName = team.TeamName;

                if (team.UpdatedById != null)
                {
                    existing.UpdatedById = team.UpdatedById;
                    existing.UpdatedAt = team.UpdatedAt;
                }
            }

            await _context.SaveChangesAsync(ct);

            return team;
        }

        public async Task<bool> SendTeamInvitationsAsync(List<string> userIds, List<int> teamIds, string userId, CancellationToken ct)
        {
            if (teamIds == null || !teamIds.Any() || userIds == null || !userIds.Any())
                return false;

            // 1. Fetch full TeamMember entities so EF Core tracks them for updates
            var existingRelations = await _context.TeamMembers
                .Where(x => userIds.Contains(x.UserId) && teamIds.Contains(x.TeamId))
                .ToListAsync(ct);

            // 2. Separate active connections from soft-deleted ones
            // We use ValueTuples (UserId, TeamId) for easy O(1) HashSet lookups
            var activeSet = existingRelations
                .Where(x => !x.IsDeleted)
                .Select(x => (x.UserId, x.TeamId))
                .ToHashSet();

            var softDeletedRelations = existingRelations
                .Where(x => x.IsDeleted)
                .ToList();

            var softDeletedSet = softDeletedRelations
                .Select(x => (x.UserId, x.TeamId))
                .ToHashSet();

            // 3. Flip IsDeleted back to false for anyone who was soft-deleted
            foreach (var relation in softDeletedRelations)
            {
                relation.IsDeleted = false;
                relation.UpdatedById = userId;
                relation.CreatedAt = DateTime.Now;
                relation.UpdatedAt = DateTime.UtcNow;
                relation.Status = TA_Domain.Enums.MembershipStatus.PendingApproval;
                // EF Core tracks this change automatically because we selected the whole entity
            }

            // 4. Generate entirely NEW entries only for pairs that don't exist AT ALL 
            // (Not active, and wasn't soft-deleted either)
            var newRelations = userIds
                .SelectMany(userId => teamIds.Select(teamId => (UserId: userId, TeamId: teamId)))
                .Where(pair => !activeSet.Contains(pair) && !softDeletedSet.Contains(pair))
                .Select(pair => new TeamMember
                {
                    UserId = pair.UserId,
                    TeamId = pair.TeamId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                })
                .ToList();

            // 5. If we didn't restore anything and have no new rows to add, we're done
            if (!softDeletedRelations.Any() && !newRelations.Any())
                return true;

            if (newRelations.Any())
            {
                await _context.TeamMembers.AddRangeAsync(newRelations, ct);
            }

            // This saves both the newly added rows AND the modified IsDeleted properties
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<List<TeamMember>> GetAllTeamMembersAsync(int companyId, CancellationToken ct)
        {
            return await _context.TeamMembers
                .Include(t => t.User)
                .Include(t => t.Team)
                .Include(t => t.CreatedBy)
                .Where(t => !t.IsDeleted &&
                            t.Team != null &&
                            t.Team.CompanyId == companyId && !t.Team.IsDeleted)
                .ToListAsync(ct);
        }
        public async Task<List<(TeamMember Member, string RoleName)>> GetTeamMembersAsync(
      int companyId,
      int? teamId,
      CancellationToken ct)
        {
            var query = _context.TeamMembers
                .Where(tm =>
                    !tm.IsDeleted &&
                    tm.Team != null &&
                    !tm.Team.IsDeleted &&
                    tm.Team.CompanyId == companyId);

            if (teamId.HasValue)
            {
                query = query.Where(tm => tm.TeamId == teamId.Value && tm.Status == MembershipStatus.Active);
            }

            var result = await (
                from tm in query
                join ur in _context.UserRoles on tm.UserId equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                select new
                {
                    Member = tm,
                    RoleName = r.Name
                }
            ).ToListAsync(ct);

            return result
                .Select(x => (x.Member, x.RoleName))
                .ToList();
        }
        public async Task<List<TeamMember>> GetAllUserTeams(string userId, CancellationToken ct)
        {
            return await _context.TeamMembers
                 .Include(t => t.User)
                .Include(t => t.Team)
                .Include(t => t.CreatedBy)
                .Where(t => !t.IsDeleted && t.UserId == userId &&
                            t.Team != null && !t.Team.IsDeleted)
                .ToListAsync();
        }

        public async Task<TeamMember> UpdateMemberStatusAsync(string userId, int teamId, MembershipStatus status, CancellationToken ct)
        {
            var existing = await _context.TeamMembers
                .FirstOrDefaultAsync(t => t.UserId == userId && t.TeamId == teamId, ct);

            if (existing == null)
            {
                return null;
            }

            existing.Status = status;
            await _context.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<List<ProjectTeam>> GetProjectsOfTeamAsync(int teamId, CancellationToken ct)
        {
            return await _context.ProjectTeams
               .Include(t => t.Project)
               .Where(t => t.TeamId == teamId && !t.IsDeleted)
               .ToListAsync(ct);
        }
    }
}
