using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class TasksRepository : ITasksRepository
    {
        private readonly ApplicationDbContext _context;
        public TasksRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TA_Domain.Entities.Task> CreateTaskAsync(TA_Domain.Entities.Task entity, CancellationToken ct)
        {
            _context.Tasks.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var existing = await GetTaskById(id, ct);
            if (existing == null)
            {
                return false;
              
            }
            existing.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<TA_Domain.Entities.Task> GetAllTasks( string userId, string role, int companyId)
        {
            var isDeveloper = role.Equals("Developer", StringComparison.OrdinalIgnoreCase);

            var query = _context.Tasks
                .Where(t =>
                    !t.IsDeleted &&
                    t.Project != null &&
                    t.Project.CompanyId == companyId);

            if (isDeveloper)
            {
                query = query.Where(x =>
                    x.TaskMembers.Any(tm =>
                        tm.UserId == userId && !tm.IsDeleted)
                    &&
                    x.Project!.UserProjects.Any(up =>
                        up.UserId == userId));
            }

            return query.OrderByDescending(t => t.TaskId);
        }
        public async Task<List<TA_Domain.Entities.Task>> GetAllTasksForInsights(List<int> projectIds, string userId, string role, CancellationToken ct)
        {
            var query = _context.Tasks
                .Where(t =>
                    !t.IsDeleted &&
                    projectIds.Contains(t.ProjectId))
                .AsQueryable();

            if (role.Equals("Developer", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x =>
                    x.TaskMembers.Any(tm =>
                        tm.UserId == userId &&
                        !tm.IsDeleted)
                    &&
                    x.Project != null &&
                    x.Project.UserProjects != null &&
                    x.Project.UserProjects.Any(up =>
                        up.UserId == userId));
            }

            return await query.ToListAsync(ct);
        }
        public async Task<List<TaskMember>> GetCompanyMembersTasks(int companyId, DateTime startDate, int? teamId, CancellationToken ct)
        {
            var query = _context.TaskMembers
                .Where(tm =>
                    !tm.IsDeleted &&
                    tm.CreatedAt >= startDate &&
                    tm.User != null &&
                    tm.User.CompanyId == companyId &&
                    !tm.User.isDeleted &&
                    tm.Task != null &&
                    !tm.Task.IsDeleted &&
                    tm.Task.Project != null &&
                    tm.Task.Project.CompanyId == companyId);

            if (teamId.HasValue)
            {
                query = query.Where(tm =>
                    tm.Task.Project.ProjectTeams
                        .Any(pt => pt.TeamId == teamId.Value));
            }

            return await query
                .Include(tm => tm.User)
                .Include(tm => tm.Task)
                .ToListAsync(ct);
        }

        public async Task<TA_Domain.Entities.Task> GetTaskByExternalIdAsync(string externalId, CancellationToken ct)
        {
            return await _context.Tasks
                .Where(t => t.ExternalId == externalId && !t.IsDeleted)
                //.Select(t => new TaskDTO
                //{
                //    TaskId = t.TaskId,
                //    Title = t.Title,
                //    Description = t.Description,
                //    Status = t.Status,
                //    Priority = t.Priority,
                //    ProjectName = t.Project != null ? t.Project.Name : null,
                //    isDeleted = t.IsDeleted,
                //    ProjectId = t.ProjectId
                //})
                .FirstOrDefaultAsync(ct);
        }
        public async Task<TA_Domain.Entities.Task> GetTaskById(int id, CancellationToken ct)
        {
            var dto = await _context.Tasks
                .Where(t => t.TaskId == id)
                .FirstOrDefaultAsync(ct);

            if (dto == null)
                throw new ArgumentException("Task not found", nameof(id));

            return dto;
        }
        public async Task<List<TaskMember>> GetTaskMembers(int taskId, CancellationToken ct)
        {
            var members = await _context.TaskMembers
                .Where(m => m.TaskId == taskId && !m.IsDeleted)
                .ToListAsync(ct);
            return members;
        }

        public async Task<bool> RemoveMember(string id, int taskId, CancellationToken ct)
        {
            var member = await _context.TaskMembers.Where(m => m.UserId == id && m.TaskId == taskId && !m.IsDeleted).FirstOrDefaultAsync();
            if (member == null)
            {
                return false;
            }
            member.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskMember?> SaveTaskMembers(TaskMember entity, CancellationToken ct)
        {
            var exists = await _context.TaskMembers
                .AnyAsync(t => t.UserId == entity.UserId
                            && t.TaskId == entity.TaskId
                            && !t.IsDeleted);

            if (exists)
            {
                throw new ArgumentException("User already assigned to task!", nameof(entity));
            }

            _context.TaskMembers.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TA_Domain.Entities.Task> UpdateTaskAsync(TA_Domain.Entities.Task dto, CancellationToken ct)
        {
            var existing = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == dto.TaskId) ?? throw new KeyNotFoundException("Task not found");

            existing.Status = dto.Status!;
            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.Priority = dto.Priority;

            await _context.SaveChangesAsync();
            return dto;
        }
    }
}
