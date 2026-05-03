using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface ITasksService
    {
        public Task<UpdateTaskDTO> UpdateAsync(UpdateTaskDTO dto);
        public Task DeleteAsync(int id);
        public Task<TaskDTO> GetTaskByIdAsync(int id);
        Task<PagedResult<TaskDTO>> GetTasksAsync(TaskQuery query, string userId, string role, int companyId);
        Task<PagedResult<TaskDTO>> GetProjectTasks(int projectId);
        Task<CompanyAssetsDTO> GetUsersAndProjectsAsync(int companyId, string role, string userId);
        Task<TaskMembersResponseDTO> GetAllMembersAsync(int taskId);
        Task<List<UserDTO>> GetTaskMembers(int taskId);
        public Task RemoveMember(string id, int taskId);
        Task<ResponseDTO> SaveJiraWebhookTask(int projectId, JiraWebhookPayload payload);
        Task<ResponseDTO> ProcessTrelloWebhookAsync(int projectId, TrelloWebhookPayload payload);

    }
}
