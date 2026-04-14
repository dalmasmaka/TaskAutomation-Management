using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;

namespace TA_Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectIntegrationRepository _integrationRepository;
        private readonly IProjectRepository _projectRepository;
        public ProjectService(IProjectIntegrationRepository integrationRepository, IProjectRepository projectRepository)
        {
            _integrationRepository = integrationRepository;
            _projectRepository = projectRepository;
        }

        public async Task AssignUserToProjects(AssignUserProjectsDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrEmpty(dto.UserId) || dto.Projects == null || !dto.Projects.Any())
                throw new ArgumentException("Invalid data");

            await _projectRepository.AssignUserToProjects(dto.UserId, dto.Projects);
        }
        public async Task AssignUsersToProject(AssignProjectUsersDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ProjectId < 1 || dto.Users == null || !dto.Users.Any())
                throw new ArgumentException("Invalid data");

            await _projectRepository.AssignUsersToProject(dto.ProjectId, dto.Users);
        }

        public async Task<List<ProjectDTO>> GetAllAsync(int companyId)
        {
            try
            {
                var projects = await _projectRepository.GetAllAsync(companyId);
                return projects;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<UserDTO>> GetMembersAsync(int projectId)
        {
            try
            {
                var members = await _projectRepository.GetMembersAsync(projectId);
                return members;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId)
        {
            try
            {
                var projects = await _projectRepository.GetProjectsAvailableToAssignToUser(companyId, userId);
                return projects;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<UserDTO>> GetUsersAvailableToAssignToProjects(int companyId, int projectId)
        {
            try
            {
                var users = await _projectRepository.GetUsersAvailableToAssignToProjects(companyId, projectId);
                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task RemoveMemberAsync(string userId, int projectId)
        {
            try
            {
                await _projectRepository.RemoveMemberAsync(userId, projectId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseDTO> SaveProjectIntegration(WholeProjectDTO project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            try
            {
                // 1. Map and Save the Project
                var projectDTO = new ProjectDTO
                {
                    Name = project.Name,
                    Platform = project.Platform,
                    RepoURl = project.RepoURl,
                };

                // Assuming AddAsync returns the created entity or we can get the new ID
                var savedProject = await _projectRepository.SaveAsync(projectDTO);

                // 2. Map and Save the Integration
                var projectIntegrationDTO = new ProjectIntegrationDTO
                {
                    // Use the ID from the newly saved project if project.ProjectId is 0
                    ProjectId = savedProject.ProjectId,
                    AccessToken = project.AccessToken,
                    Provider = project.Provider,
                    RefreshToken = project.RefreshToken,
                    ExpiresAt = project.ExpiresAt,
                    ExternalId = project.ExternalId,
                };

                await _integrationRepository.SaveAsync(projectIntegrationDTO);

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Project and Integration saved successfully."
                };
            }
            catch (Exception ex)
            {
                // Log the error (Always log!)
                // _logger.LogError(ex, "Error saving project integration");

                return new ResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while saving the project."
                };
            }
        }
    }
}
