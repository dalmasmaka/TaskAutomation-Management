using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface ITasksRepository
    {
        Task<TA_Domain.Entities.Task> UpdateTaskAsync(TA_Domain.Entities.Task entity, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
        Task<TA_Domain.Entities.Task> GetTaskById(int id, CancellationToken ct);
        Task<TA_Domain.Entities.Task> CreateTaskAsync(TA_Domain.Entities.Task entity, CancellationToken ct);
        IQueryable<TA_Domain.Entities.Task> GetAllTasks(string userId, string role, int companyId);
        Task<List<TA_Domain.Entities.Task>> GetAllTasksForInsights(List<int> projectIds, string userId, string role, CancellationToken ct);
        Task<List<TaskMember>> GetTaskMembers(int taskId, CancellationToken ct);
        Task<TaskMember?> SaveTaskMembers(TaskMember entity, CancellationToken ct);
        Task<bool> RemoveMember(string id, int taskId, CancellationToken ct);
        Task<TA_Domain.Entities.Task> GetTaskByExternalIdAsync(string externalId, CancellationToken ct);
        Task<List<TaskMember>> GetCompanyMembersTasks(int companyId, DateTime startDate, int? teamId, CancellationToken ct);
    }
}
