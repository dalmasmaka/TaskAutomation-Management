using Microsoft.Extensions.Logging;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectIntegrationRepository _integrationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<ProjectService> _logger;
        public ProjectService(IProjectIntegrationRepository integrationRepository, IProjectRepository projectRepository, ILogger<ProjectService> logger, ITeamRepository teamRepository)
        {
            _integrationRepository = integrationRepository;
            _projectRepository = projectRepository;
            _logger = logger;
            _teamRepository = teamRepository;
        }

        public async Task<ResponseDTO> AssignUserToProjects(AssignUserProjectsDTO dto, CancellationToken ct)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            try
            {
                if (string.IsNullOrEmpty(dto.UserId) || dto.Projects == null || !dto.Projects.Any())
                    throw new ArgumentException("Invalid data");

                var result = await _projectRepository.AssignUserToProjects(dto.UserId, dto.Projects, ct);
                if (!result)
                {
                    _logger.LogWarning("Failed to assign user to projects. UserId: {UserId}", dto.UserId);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Failed to assign user to projects."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "User assigned to projects successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Assignment operation for user {UserId} was canceled by the client.", dto.UserId);
                throw;
            }

            catch (Exception ex)
            {
                _logger.LogWarning("Failed to assign user to projects. UserId: {UserId}", dto.UserId);
                throw;
            }
        }
        public async Task<ResponseDTO> AssignUsersToProject(AssignProjectUsersDTO dto, CancellationToken ct)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ProjectId < 1 || dto.Users == null || !dto.Users.Any())
                throw new ArgumentException("Invalid project data or empty user list.", nameof(dto));

            try
            {
                var result = await _projectRepository.AssignUsersToProject(dto.ProjectId, dto.Users, ct);

                if (!result)
                {
                    _logger.LogWarning("Repository failed to assign users to project {ProjectId}.", dto.ProjectId);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Failed to assign users to the project. The project may not exist."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Users assigned to the project successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Assignment operation for project {ProjectId} was canceled by the client.", dto.ProjectId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while assigning users to project {ProjectId}.", dto.ProjectId);
                throw;
            }
        }
        public async Task<List<ProjectDTO>> GetAllAsync(int companyId, string userId, CancellationToken ct)
        {
            try
            {
                var projects = await _projectRepository.GetAllAsync(companyId, userId, ct);

                var mapped = projects.Select(t =>
                {
                    var config = t.RepositoryConfigs?.FirstOrDefault();

                    return new ProjectDTO
                    {
                        ProjectId = t.ProjectId,
                        Name = t.Name,
                        CompanyId = t.CompanyId,
                        HasConfig = t.ProjectIntegrations != null && t.ProjectIntegrations.Any(),
                        CreatedAt = t.CreatedAt.ToString("dd MMMM yyyy"),
                        CreatedBy = t.CreatedBy?.FullName,
                        TotalUsers = t.UserProjects?
                            .Count(up => !up.IsDeleted && up.User != null && !up.User.isDeleted) ?? 0,
                        TotalTasks = t.Tasks?
                            .Count(task => !task.IsDeleted) ?? 0,
                        IsAssigned = t.UserProjects?
                            .Any(up => up.UserId == userId && !up.IsDeleted) ?? false,
                        Provider = config?.Provider,
                        RepoName = config?.RepoName,
                        RepoOwner = config?.RepoOwner,
                        DefaultBranch = config?.DefaultBranch,
                        AccessToken = config?.AccessToken,
                    };
                }).ToList();

                return mapped;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Fetching projects operation for CompanyId: {CompanyId} was canceled by the client.", companyId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects for CompanyId: {CompanyId}, UserId: {UserId}", companyId, userId);
                throw;
            }
        }

        public async Task<List<UserDTO>> GetMembersAsync(int projectId, CancellationToken ct)
        {
            try
            {
                var members = await _projectRepository.GetMembersAsync(projectId, ct);

                var mappedList = members.Select(member => new UserDTO
                {
                    Id = member.User.Id,
                    FullName = member.User.FullName,
                    UserName = member.User.UserName,
                    Email = member.User.Email,
                    PhoneNumber = member.User.PhoneNumber,
                    Role = member.RoleName
                })
                .ToList();

                return mappedList;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Fetching members operation for ProjectId: {ProjectId} was canceled by the client.", projectId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching members for ProjectId: {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId, CancellationToken ct)
        {
            try
            {
                var projects = await _projectRepository.GetProjectsAvailableToAssignToUser(companyId, userId, ct);
                var mappedList = projects.Select(project => new ProjectDTO
                {
                    ProjectId = project.ProjectId,
                    Name = project.Name,
                })
                 .ToList();
                return mappedList;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Fetching projects available to assign to the user operation for CompanyId: {CompanyId} was canceled by the client.", companyId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects available to assign to user!");
                throw;
            }
        }

        public async Task<List<UserDTO>> GetUsersAvailableToAssignToProject(int companyId, int projectId, CancellationToken ct)
        {
            try
            {
                var users = await _projectRepository.GetUsersAvailableToAssignToProject(companyId, projectId, ct);
                var mappedList = users.Select(member => new UserDTO
                {
                    Id = member.User.Id,
                    FullName = member.User.FullName,
                    Email = member.User.Email,
                })
               .ToList();

                return mappedList;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Fetching users available to assign to projects operation for CompanyId: {CompanyId} was canceled by the client.", companyId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users available to assign to projects!");
                throw;
            }
        }

        public async Task<ResponseDTO> RemoveMemberAsync(string userId, int projectId, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            try
            {
                var result = await _projectRepository.RemoveMemberAsync(userId, projectId, ct);
                if (!result)
                {
                    _logger.LogWarning("Failed to remove member {UserId} from project {ProjectId}.", userId, projectId);

                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Failed to remove member from project."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Member removed successfully from project."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Removing member {UserId} operation for ProjectId: {ProjectId} was canceled.", userId, projectId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member {UserId} from project {ProjectId}.", userId, projectId);
                throw;
            }
        }
        public async Task<ResponseDTO> SaveProjectAsync(string userId, int companyId, CreateProjectDTO project, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(project);

            try
            {
                var entity = new TA_Domain.Entities.Project
                {
                    ProjectId = project.ProjectId,
                    Name = project.Name,
                    CompanyId = companyId,
                    CreatedAt = DateTime.Now,
                    CreatedById = userId

                };
                var integration = new GitHubConfig
                {
                    ProjectId = project.ProjectId,
                    Provider = project.Provider,
                    RepoName = project.RepoName,
                    RepoOwner = project.RepoOwner,
                    DefaultBranch = project.DefaultBranch,
                    AccessToken = project.AccessToken,
                    CreatedAt = DateTime.Now,
                    CreatedById = userId
                };
                

                var savedProject = await _projectRepository.SaveAsync(entity, ct);
                integration.ProjectId = savedProject.ProjectId;
                var saveIntegration = await _projectRepository.SaveGitHubIntegrationAsync(integration, ct);
                var saveInTeams = await _teamRepository.AddProjectToTeamsAsync(savedProject.ProjectId, project.Teams, userId, ct);
                return new ResponseDTO
                {
                    Success = true,
                    Message = "Project and Integration saved successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Saving project operation for CompanyId: {CompanyId} was canceled.", companyId);
                throw;
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

        public async Task<List<ProjectDTO>> GetUserProjectsAsync(int companyId, string userId, CancellationToken ct)
        {
            try
            {
                if (userId == null) { throw new ArgumentNullException(nameof(userId)); }
                var projects = await _projectRepository.GetProjectsByUserIdAsync(companyId, userId, ct);

                var mappedList = projects.Select(project => new ProjectDTO
                {
                    ProjectId = project.ProjectId,
                    Name = project.Name,
                })
               .ToList();

                return mappedList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects!");
                throw;
            }
        }

        public async Task<ProjectDTO> GetProjectByExternalId(string externalId, CancellationToken ct)
        {
            try
            {
                if (externalId == null) throw new ArgumentNullException(nameof(externalId));
                var project = await _integrationRepository.GetProjectByExternalId(externalId, ct);
                var dto = new ProjectDTO
                {
                    ProjectId = project.Id
                };
                return dto;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching projects!");
                throw;
            }
        }
    }
}
