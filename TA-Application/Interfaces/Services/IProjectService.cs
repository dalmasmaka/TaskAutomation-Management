using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<ResponseDTO> SaveProjectAsync(string userId, int companyId, CreateProjectDTO project);
        Task<List<ProjectDTO>> GetAllAsync(int companyId, string userId);
        Task<List<ProjectDTO>> GetUserProjectsAsync(int companyId, string userId);
        //projektet ne te cilat nje user i caktum nuk eshte assigned ende ne to 
        Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId);
        //userat qe nuk jon te bom assign ne nje projekt te caktum 
        Task<List<UserDTO>> GetUsersAvailableToAssignToProjects(int companyId, int projectId);
        Task AssignUserToProjects(AssignUserProjectsDTO dto);
        Task<ResponseDTO> AssignUsersToProject(AssignProjectUsersDTO dto);
        Task<List<UserDTO>> GetMembersAsync(int projectId);
        Task RemoveMemberAsync(string userId, int projectId);
    }
}
