using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface IProjectRepository
    {
        Task<CreateProjectDTO> SaveAsync(CreateProjectDTO project);
        Task<CreateProjectDTO> SaveIntegrationAsync(CreateProjectDTO project);
        Task DeleteAsync(int id);
        Task<List<ProjectDTO>> GetAllAsync(int companyId, string userId);
        Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId);
        Task<List<UserDTO>> GetUsersAvailableToAssignToProjects(int companyId, int projectId);
        Task AssignUserToProjects(string userId, List<int> projectIds);
        Task<ResponseDTO> AssignUsersToProject(int projectId, List<string> userIds);
        Task<List<UserDTO>> GetMembersAsync(int projectId);
        Task RemoveMemberAsync(string userId, int projectId);
        Task <List<ProjectDTO>> GetProjectsByUserIdAsync(int companyId, string userId);
        Task<RepoDTO> GetRepoAsync(int projectId);

    }
}
