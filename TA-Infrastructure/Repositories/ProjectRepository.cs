using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Domain.Enums;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;
        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Project> SaveAsync(Project project, CancellationToken ct)
        {
            var existing = await _context.Projects
                .FirstOrDefaultAsync(pi => pi.ProjectId == project.ProjectId, ct);

            if (existing != null)
            {
                existing.Name = project.Name!;
                existing.CompanyId = project.CompanyId;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedById = project.UpdatedById;

                await _context.SaveChangesAsync(ct);
                return existing;
            }
            else
            {
                await _context.Projects.AddAsync(project, ct);
                await _context.SaveChangesAsync(ct);
                return project;
            }
        }
        public async Task<GitHubConfig> SaveGitHubIntegrationAsync(GitHubConfig github, CancellationToken ct)
        {
            var existing = await _context.GitHubConfigs
                .FirstOrDefaultAsync(pi => pi.ProjectId == github.ProjectId, ct);

            if (existing != null)
            {
                existing.Provider = github.Provider;
                existing.RepoName = github.RepoName;
                existing.RepoOwner = github.RepoOwner;
                existing.DefaultBranch = github.DefaultBranch;
                existing.AccessToken = github.AccessToken;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedById = github.UpdatedById;

                await _context.SaveChangesAsync(ct);
                return github;
            }
            else
            {

                await _context.GitHubConfigs.AddAsync(github);
                await _context.SaveChangesAsync(ct);
                return github;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _context.Projects
                 .FirstOrDefaultAsync(p => p.ProjectId == id, ct);

            if (existing == null)
            {
                return false;
                
            }
            existing.IsDeleted = true;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<Project>> GetAllAsync(int companyId, string userId, CancellationToken ct)
        {
            var projects = await _context.Projects
             .Where(p => !p.IsDeleted && p.CompanyId == companyId)
             .OrderByDescending(p => p.ProjectId)
             .Select(p => new Project
             {
                 ProjectId = p.ProjectId,
                 Name = p.Name,
                 CompanyId = p.CompanyId,

                 RepositoryConfigs = p.RepositoryConfigs,
                 ProjectIntegrations = p.ProjectIntegrations,

                 CreatedBy = p.CreatedBy,

                 UserProjects = p.UserProjects
                     .Where(up => !up.IsDeleted)
                     .ToList(),

                 Tasks = p.Tasks
                     .Where(t => !t.IsDeleted)
                     .ToList(),

             })
             .ToListAsync(ct);
            return projects;
        }
        public async Task<List<Project>> GetProjectsForInsights(int companyId, int? teamId, CancellationToken ct)
        {
            var query = _context.Projects
                .Where(p => !p.IsDeleted && p.CompanyId == companyId)
                .Include(p => p.ProjectTeams)
                .Include(t => t.UserProjects)
                .ThenInclude(up => up.User)
                .AsQueryable();

            if (teamId.HasValue)
            {
                query = query.Where(p =>
                    p.ProjectTeams.Any(pt => pt.TeamId == teamId.Value));
            }

            return await query.ToListAsync(ct);
        }
        public async Task<List<Project>> GetProjectsAvailableToAssignToUser(int companyId, string userId, CancellationToken ct)
        {
            // Get all teams where the user is an active member
            var teamIds = await _context.TeamMembers
                .Where(tm =>
                    !tm.IsDeleted &&
                    tm.UserId == userId &&
                    tm.Status == MembershipStatus.Active)
                .Select(tm => tm.TeamId)
                .ToListAsync(ct);

            // Get projects connected to those teams
            var projectIds = await _context.ProjectTeams
                .Where(pt =>
                    !pt.IsDeleted &&
                    teamIds.Contains(pt.TeamId))
                .Select(pt => pt.ProjectId)
                .Distinct()
                .ToListAsync(ct);

            // Return only projects:
            // - from the user's teams
            // - not deleted
            // - from the same company
            // - not already assigned to the user
            var projects = await _context.Projects
                .Include(p => p.ProjectTeams)
                .Where(p =>
                    !p.IsDeleted &&
                    p.CompanyId == companyId &&
                    projectIds.Contains(p.ProjectId) &&
                    !p.UserProjects!.Any(up =>
                        up.UserId == userId &&
                        !up.IsDeleted))
                .ToListAsync(ct);

            return projects;
        }
        public async Task<List<TeamMember>> GetUsersAvailableToAssignToProject(int companyId, int projectId, CancellationToken ct)
        {
            var teamIds = await _context.ProjectTeams
                .Where(pt => pt.ProjectId == projectId && !pt.IsDeleted)
                .Select(pt => pt.TeamId)
                .ToListAsync(ct);

            var members = await (
                from tm in _context.TeamMembers
                join ur in _context.UserRoles on tm.UserId equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                where !tm.IsDeleted
                      && tm.Status == MembershipStatus.Active
                      && teamIds.Contains(tm.TeamId)

                      && tm.User != null
                      && tm.User.CompanyId == companyId
                      && !tm.User.isDeleted

                      && r.Name != "Owner"

                      && !tm.User.UserProjects!.Any(up =>
                             up.ProjectId == projectId &&
                             !up.IsDeleted)
                select tm
            )
            .Include(tm => tm.User)
            //.Include(tm => tm.Team)
            .Distinct()
            .ToListAsync(ct);

            return members;
        }

        public async Task<bool> AssignUserToProjects(string userId, List<int> projectIds, CancellationToken ct)
        {
            if (projectIds == null || !projectIds.Any())
            {
                return false;
            }

            var existingProjectIds = await _context.UserProjects
                .Where(up => up.UserId == userId && projectIds.Contains(up.ProjectId))
                .Select(up => up.ProjectId)
                .ToListAsync(ct);

            var existingSet = new HashSet<int>(existingProjectIds);
            var newProjectIds = projectIds.Distinct().Where(id => !existingSet.Contains(id));

            if (!newProjectIds.Any())
            {
                return false; 
            }

            var newAssignments = newProjectIds.Select(projectId => new UserProject
            {
                UserId = userId,
                ProjectId = projectId
            });

            await _context.UserProjects.AddRangeAsync(newAssignments, ct);

            await _context.SaveChangesAsync(ct);
            return true;
        }
        public async Task<bool> AssignUsersToProject(int projectId, List<string> userIds, CancellationToken ct)
        {
            if (userIds == null || !userIds.Any())
            {
                return false;
            }
            // Deduplicate input list to prevent processing the same user twice
            var distinctUserIds = userIds.Distinct().ToList();

            // 1. Single database query to fetch ALL existing relations (including soft-deleted ones)
            // Note: If you have a Global Query Filter hiding IsDeleted, use _context.UserProjects.IgnoreQueryFilters()
            var existingAssignments = await _context.UserProjects
                .Where(up => up.ProjectId == projectId && distinctUserIds.Contains(up.UserId))
                .ToDictionaryAsync(up => up.UserId, ct);

            var newAssignments = new List<UserProject>();

            // 2. Process everything efficiently in-memory
            foreach (var userId in distinctUserIds)
            {
                if (existingAssignments.TryGetValue(userId, out var userProject))
                {
                    // State 2: It exists and is soft-deleted -> Restore it
                    if (userProject.IsDeleted)
                    {
                        userProject.IsDeleted = false; // EF tracks this change automatically
                        userProject.UpdatedAt = DateTime.UtcNow; // Optional: if you track update times
                    }
                    // State 3: It exists and is already active -> Do nothing
                }
                else
                {
                    // State 1: It doesn't exist -> Queue for creation
                    newAssignments.Add(new UserProject
                    {
                        UserId = userId,
                        ProjectId = projectId,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            // 3. Batch insert new records if any exist
            if (newAssignments.Any())
            {
                await _context.UserProjects.AddRangeAsync(newAssignments, ct);
            }

            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<List<(ApplicationUser User, string RoleName)>> GetMembersAsync(int projectId, CancellationToken ct)
        {
            var members = await (
                from up in _context.UserProjects
                join user in _context.Users on up.UserId equals user.Id
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where up.ProjectId == projectId
                   && !up.IsDeleted
                   && !user.isDeleted 
                select new
                {
                    User = user,
                    RoleName = role.Name
                }
            ).ToListAsync(ct);

            return members.Select(m => (m.User, m.RoleName)).ToList();
        }
        public async Task<bool> RemoveMemberAsync(string userId, int projectId, CancellationToken ct)
        {
            var existingMember = await _context.UserProjects.Where(m => m.UserId == userId && m.ProjectId == projectId).FirstOrDefaultAsync();
            if (existingMember == null)
            {
                return false;
            }
            existingMember.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Project>> GetProjectsByUserIdAsync(int companyId, string userId, CancellationToken ct)
        {
            return await _context.Projects
                .Where(p => !p.IsDeleted &&
                            p.CompanyId == companyId &&
                            p.UserProjects!.Any(up => up.UserId == userId && !up.IsDeleted)) 
                .ToListAsync(ct);
        }
        public async Task<GitHubConfig?> GetRepoAsync(int projectId, CancellationToken ct)
        {
            return await _context.GitHubConfigs
                .FirstOrDefaultAsync(g => g.ProjectId == projectId, ct);
        }

        public async Task<List<Project>> GetAllByCompanyIdAsync(int companyId, CancellationToken ct)
        {
            return await _context.Projects
               .Where(p => !p.IsDeleted && p.CompanyId == companyId)
               .OrderByDescending(p => p.ProjectId)
               .ToListAsync(ct);
        }
    }
}
