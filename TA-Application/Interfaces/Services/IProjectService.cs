using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<ResponseDTO> SaveProjectIntegration(WholeProjectDTO project);
        Task<List<ProjectDTO>> GetAllAsync(int companyId);
        //projektet ne te cilat nje user i caktum nuk eshte assigned ende ne to 
        Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId);
        Task<List<UserDTO>> GetUsersAvailableToAssignToProjects(int companyId, int projectId);
        Task AssignUserToProjects(AssignUserProjectsDTO dto);
        Task AssignUsersToProject(AssignProjectUsersDTO dto);
        Task<List<UserDTO>> GetMembersAsync(int projectId);
        Task RemoveMemberAsync(string userId, int projectId);
    }
}
