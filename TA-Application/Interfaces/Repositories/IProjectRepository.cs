using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        Task<ProjectDTO> SaveAsync(ProjectDTO project);
        Task DeleteAsync(int id);
        Task<List<ProjectDTO>> GetAllAsync(int companyId);
        Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId);
        Task<List<UserDTO>> GetUsersAvailableToAssignToProjects(int companyId, int projectId);
        Task AssignUserToProjects(string userId, List<int> projectIds);
        Task AssignUsersToProject(int projectId, List<string> userIds);
        Task<List<UserDTO>> GetMembersAsync(int projectId);
        Task RemoveMemberAsync(string userId, int projectId);

    }
}
