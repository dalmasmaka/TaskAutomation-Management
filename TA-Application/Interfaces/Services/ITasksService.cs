using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface ITasksService
    {
        public Task<TaskDTO> UpdateAsync(UpdateTaskDTO dto, CancellationToken ct);
        public Task<ResponseDTO> DeleteAsync(int id, CancellationToken ct);
        public Task<TaskDTO> GetTaskByIdAsync(int id, CancellationToken ct);
        Task<PagedResult<TaskDTO>> GetTasksAsync(TaskQuery query, string userId, string role, int companyId, CancellationToken ct);
        Task<CompanyAssetsDTO> GetUsersAndProjectsAsync(int companyId, string role, string userId, CancellationToken ct);
        Task<TaskMembersResponseDTO> GetAllMembersAsync(int taskId, CancellationToken ct);
        Task<List<UserDTO>> GetTaskMembers(int taskId, CancellationToken ct);
        public Task<ResponseDTO> RemoveMember(string id, int taskId, CancellationToken ct);
        Task<ResponseDTO> SaveJiraWebhookTask(int projectId, JiraWebhookPayload payload, CancellationToken ct);
        Task<ResponseDTO> ProcessTrelloWebhookAsync(int projectId, TrelloWebhookPayload payload, CancellationToken ct);

    }
}
