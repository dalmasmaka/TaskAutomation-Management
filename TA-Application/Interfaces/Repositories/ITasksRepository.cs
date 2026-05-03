using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface ITasksRepository
    {
        Task<UpdateTaskDTO> UpdateTaskAsync(UpdateTaskDTO dto);
        Task DeleteAsync(int id);
        Task<TaskDTO> GetTaskById(int id);
        Task<TaskDTO> CreateTaskAsync(TaskDTO dto);
        IQueryable<TaskDTO> GetAllTasks(string userId, string role, int companyId);
        IQueryable<TaskDTO> GetProjectTasks(int projectId);
        Task<List<UserDTO>> GetTaskMembers(int taskId);
        Task<TaskMembersDTO?> SaveTaskMembers(TaskMembersDTO dto);
        Task RemoveMember(string id, int taskId);
        Task<TaskDTO> GetTaskByExternalIdAsync(string externalId);
    }
}
