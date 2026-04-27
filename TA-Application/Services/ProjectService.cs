using Microsoft.Extensions.Logging;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;

namespace TA_Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectIntegrationRepository _integrationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ProjectService> _logger;
        public ProjectService(IProjectIntegrationRepository integrationRepository, IProjectRepository projectRepository, ILogger<ProjectService> logger)
        {
            _integrationRepository = integrationRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task AssignUserToProjects(AssignUserProjectsDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                if (string.IsNullOrEmpty(dto.UserId) || dto.Projects == null || !dto.Projects.Any())
                    throw new ArgumentException("Invalid data");

                await _projectRepository.AssignUserToProjects(dto.UserId, dto.Projects);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error assigning user to projects!");
                throw;
            }
        }
        public async Task<ResponseDTO> AssignUsersToProject(AssignProjectUsersDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ProjectId < 1 || dto.Users == null || !dto.Users.Any())
                throw new ArgumentException("Invalid data");

            try
            {
                var result = await _projectRepository.AssignUsersToProject(dto.ProjectId, dto.Users);

                return result; 
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error assigning users to project!");
                throw;
            }
        }

        public async Task<List<ProjectDTO>> GetAllAsync(int companyId, string userId)
        {
            try
            {
                var projects = await _projectRepository.GetAllAsync(companyId, userId);
                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects!");
                throw;
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
                _logger.LogError(ex, "Error fetching members!");
                throw;
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
                _logger.LogError(ex, "Error fetching projects available to assign to user!");
                throw;
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
                _logger.LogError(ex, "Error fetching users available to assign to projects!");
                throw;
            }
        }

        public async System.Threading.Tasks.Task RemoveMemberAsync(string userId, int projectId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            try
            {
                await _projectRepository.RemoveMemberAsync(userId, projectId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member!");
                throw;
            }
        }

        public async Task<ResponseDTO> SaveProjectAsync(string userId, int companyId, CreateProjectDTO project)
        {
            ArgumentNullException.ThrowIfNull(project);

            try
            {
                var projectDTO = new CreateProjectDTO
                {
                    Name = project.Name,
                    Platform = project.Platform,
                    RepoURl = project.RepoURl,
                    CompanyId = companyId,
                    CreatedTime = DateTime.Now,
                    UserId = userId,
                };

                var savedProject = await _projectRepository.SaveAsync(projectDTO);

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Project and Integration saved successfully."
                };
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error saving project integration");

                return new ResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while saving the project."
                };
            }
        }

        public async Task<List<ProjectDTO>> GetUserProjectsAsync(int companyId, string userId)
        {
            try
            {
                if(userId == null) {  throw new ArgumentNullException(nameof(userId)); }
                var projects = await _projectRepository.GetProjectsByUserIdAsync(companyId, userId);
                return projects;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects!");
                throw;
            }
        }
    }
}
