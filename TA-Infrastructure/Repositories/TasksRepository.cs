using Microsoft.EntityFrameworkCore;
using TA_Application.DTOs;
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

        public async System.Threading.Tasks.Task DeleteAsync(int id)
        {
            var existing = await GetTaskById(id);
            if (existing != null)
            {
                existing.isDeleted = true;
            }
            await _context.SaveChangesAsync();
        }

        public IQueryable<TaskDTO> GetAllTasks(string userId, string role, int companyId)
        {
            var tasksQuery = _context.Tasks
                .Where(t => !t.IsDeleted)
                .OrderByDescending(i => i.TaskId)
                .AsQueryable();

            if (role == "Owner")
            {
                tasksQuery = tasksQuery
                    .Where(x => x.Project!.CompanyId == companyId);
            }
            else if (role == "Developer")
            {
                tasksQuery = tasksQuery
                    .Where(x => x.TaskMembers
                        .Any(tm => tm.UserId == userId && !tm.IsDeleted)
                        && x.Project!.UserProjects!
                            .Any(up => up.UserId == userId));
            }

            var dtoQuery = tasksQuery.Select(t => new TaskDTO
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority,
                Status = t.Status,
                ProjectId = t.ProjectId,
                ProjectName = t.Project != null ? t.Project.Name: null,
                CompanyId = t.Project != null ? t.Project.CompanyId : null,
                Assigners = t.TaskMembers
                    .Where(tm => !tm.IsDeleted)
                    .Select(tm => new UserDTO
                    {
                        Id = tm.User != null ? tm.User.Id : null,
                        FullName = tm.User != null ? tm.User.FullName : null,
                        Email = tm.User != null ? tm.User.Email : null,
                    })
                    .ToList()
            });

            return dtoQuery;
        }


        public IQueryable<TaskDTO> GetProjectTasks(int projectId)
        {
            var dtoQuery = _context.Tasks
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .Select(s => new TaskDTO
                {
                    ProjectId = s.ProjectId,
                    Title = s.Title,
                    Description = s.Description,
                    Status = s.Status
                });

            return dtoQuery;
        }

        public async Task<TaskDTO> GetTaskById(int id)
        {
            var dto = await _context.Tasks
                .Where(t => t.TaskId == id)
                .Select(t => new TaskDTO
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    ProjectName = t.Project != null ? t.Project.Name : null,
                    isDeleted = t.IsDeleted,
                    ProjectId = t.ProjectId
                })
                .FirstOrDefaultAsync();

            if (dto == null)
                throw new ArgumentException("Task not found", nameof(id));

            return dto;
        }
        public async Task<List<UserDTO>> GetTaskMembers(int taskId)
        {
            var members = await _context.TaskMembers
                .Where(m => m.TaskId == taskId && !m.IsDeleted)
                .Select(t => new UserDTO
                {
                    Id = t.UserId,
                    FullName = t.User != null ? t.User.FullName : null,
                    Email = t.User != null ? t.User.Email : null,
                }).ToListAsync();
            return members;
        }

        public async System.Threading.Tasks.Task RemoveMember(string id, int taskId)
        {
            var member = await _context.TaskMembers.Where(m => m.UserId == id && m.TaskId == taskId && !m.IsDeleted).FirstOrDefaultAsync();
            if (member == null) throw new ArgumentException("Member not found", nameof(member));
            member.IsDeleted = true;
            await _context.SaveChangesAsync();


        }

        public async Task<TaskMembersDTO?> SaveTaskMembers(TaskMembersDTO dto)
        {
            var exists = await _context.TaskMembers
                .AnyAsync(t => t.UserId == dto.UserId
                            && t.TaskId == dto.TaskId
                            && !t.IsDeleted);

            if (exists)
            {
                throw new ArgumentException("User already assigned to task!", nameof(dto));
            }

            var entity = new TaskMember
            {
                UserId = dto.UserId,
                TaskId = dto.TaskId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.TaskMembers.Add(entity);
            await _context.SaveChangesAsync();

            return new TaskMembersDTO
            {
                UserId = entity.UserId,
                TaskId = entity.TaskId
            };
        }

        public async Task<UpdateTaskDTO> UpdateTaskAsync(UpdateTaskDTO dto)
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
