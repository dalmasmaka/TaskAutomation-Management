using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class TasksService : ITasksService
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<TasksService> _logger;
        public TasksService(ITasksRepository tasksRepository, IProjectRepository projectRepository, ICompanyRepository companyRepository, ILogger<TasksService> logger)
        {
            _tasksRepository = tasksRepository;
            _projectRepository = projectRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }
        public async Task<ResponseDTO> DeleteAsync(int id, CancellationToken ct)
        {
            try
            {
                if (id == 0)
                    throw new ArgumentException("Id is null", nameof(id));

                var result = await _tasksRepository.DeleteAsync(id, ct);

                if (result == null)
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Error while deleting task."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Task deleted successfully."
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteAsync was cancelled. TaskId: {TaskId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task!");

                return new ResponseDTO
                {
                    Success = false,
                    Message = "An unexpected error occurred while deleting the task."
                };
            }
        }
        public async Task<TaskDTO> GetTaskByIdAsync(int id, CancellationToken ct)
        {
            try
            {
                if (id == 0) throw new ArgumentException("Task id is null", nameof(id));
                var task = await _tasksRepository.GetTaskById(id, ct);
                var dto = new TaskDTO
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    Priority = task.Priority,
                    ProjectName = task.Project != null ? task.Project.Name : null,
                    isDeleted = task.IsDeleted,
                    ProjectId = task.ProjectId
                };
                return dto;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteAsync was cancelled. TaskId: {TaskId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task!");
                return new TaskDTO();
            }
        }
        public async Task<TaskMembersResponseDTO> GetAllMembersAsync(int taskId, CancellationToken ct)
        {
            try
            {
                var task = await _tasksRepository.GetTaskById(taskId, ct);
                if (task == null)
                    throw new Exception("Task not found");
                var assignedMembers = await _tasksRepository.GetTaskMembers(taskId, ct);
                var projectMembers = await _projectRepository.GetMembersAsync(task.ProjectId, ct);
                var assignedIds = assignedMembers.Select(a => a.UserId).ToHashSet();

                var availableMembers = projectMembers
                    .Where(pm => !assignedIds.Contains(pm.User.Id))
                    .ToList();
                var mappedAvailableMembers = availableMembers.Select(t => new UserDTO
                {
                    UserName = t.User.UserName,
                    FullName = t.User.FullName,
                    Email = t.User.Email,
                    PhoneNumber = t.User.PhoneNumber,
                    Role = t.RoleName,

                }).ToList();
                var mappedAssignedMembers = assignedMembers.Select(t => new UserDTO
                {
                    Id = t.UserId,
                    FullName = t.User.FullName,
                    Email = t.User.Email,
                }).ToList();
                return new TaskMembersResponseDTO
                {
                    AssignedMembers = mappedAssignedMembers,
                    AvailableMembers = mappedAvailableMembers
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetAllMembersAsync was cancelled. TaskId: {TaskId}", taskId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching members!");
                return new TaskMembersResponseDTO();
            }
        }
        public async Task<PagedResult<TaskDTO>> GetTasksAsync(TaskQuery query, string userId, string role, int companyId, CancellationToken ct)
        {
            var tasks = _tasksRepository.GetAllTasks(userId, role, companyId).AsNoTracking();
            var dtoQuery = tasks.Select(t => new TaskDTO
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority,
                Status = t.Status,
                ProjectId = t.ProjectId,
                ProjectName = t.Project != null ? t.Project.Name : null,
                CompanyId = t.Project != null ? t.Project.CompanyId : null,
                Assigners = t.TaskMembers
                   .Where(tm => !tm.IsDeleted)
                   .Select(tm => new UserDTO
                   {
                       Id = tm.User != null ? tm.User.Id : null,
                       FullName = tm.User != null ? tm.User.FullName : null,
                       Email = tm.User != null ? tm.User.Email : null,
                   })
                   .ToList()
            });
            if (query.ProjectIds?.Any() == true)
            {
                dtoQuery = dtoQuery.Where(t => query.ProjectIds.Contains(t.ProjectId));
            }

            if (query.CompanyId.HasValue)
                dtoQuery = dtoQuery.Where(x => x.CompanyId == query.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var statusList = query.Status
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());

                dtoQuery = dtoQuery.Where(t => statusList.Contains(t.Status));
            }

            if (query.AssignedToIds?.Any() == true)
            {
                dtoQuery = dtoQuery.Where(t =>
                    t.Assigners!.Any(a => query.AssignedToIds.Contains(a.Id!))
                );
            }
            if (!string.IsNullOrWhiteSpace(query.Priority))
            {
                var priorityList = query.Priority
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim());

                dtoQuery = dtoQuery.Where(t => priorityList.Contains(t.Priority));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
                dtoQuery = dtoQuery.Where(x => x.Title!.Contains(query.Search));

            var totalCount = await dtoQuery.CountAsync();

            var data = await dtoQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<TaskDTO>
            {
                Items = data,
                TotalCount = totalCount
            };
        }
        public async Task<CompanyAssetsDTO> GetUsersAndProjectsAsync(int companyId, string role, string userId, CancellationToken ct)
        {
            try
            {
                List<ProjectDTO> projects = new();
                List<UserDTO> users = new();

                if (role == "Owner")
                {
                    // Get all company projects
                    var projectEntities = await _projectRepository.GetAllAsync(companyId, userId, ct);
                    projects = projectEntities.Select(t =>
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
                            AccessToken = config?.AccessToken
                        };
                    }).ToList();
                    var userEntities = await _companyRepository.GetTeamOfCompany(companyId, ct);
                    users = userEntities.Select(u => new UserDTO
                    {
                        Id = u.User.Id,
                        FullName = u.User.FullName,
                        UserName = u.User.UserName,
                        PhoneNumber = u.User.PhoneNumber,
                        Role = u.RoleName
                    }).ToList();
                }
                else if (role == "Developer")
                {
                    var projectEntities = await _projectRepository.GetAllAsync(companyId, userId, ct);
                    projects = projectEntities.Select(t =>
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
                            AccessToken = config?.AccessToken
                        };
                    }).ToList();
                }

                return new CompanyAssetsDTO
                {
                    Projects = projects,
                    Assigners = users
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetUsersAndProjectsAsync was cancelled. CompanyId: {CompanyId}", companyId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching members!");
                return new CompanyAssetsDTO();
            }
        }
        public async Task<TaskDTO> UpdateAsync(UpdateTaskDTO dto, CancellationToken ct)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Task is null");
            var entity = new TA_Domain.Entities.Task
            {
                TaskId = dto.TaskId,
                Status = dto.Status,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,

            };
            var task = await _tasksRepository.UpdateTaskAsync(entity, ct);
            if (dto.Members != null && dto.Members.Any())
            {
                foreach (var userId in dto.Members)
                {
                    var memberEntity = new TaskMember
                    {
                        TaskId = dto.TaskId,
                        UserId = userId
                    };

                    await _tasksRepository.SaveTaskMembers(memberEntity, ct);
                }
            }
            var mapped = new TaskDTO
            {
                Status = dto.Status,
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
            };
            return mapped;
        }
        public async Task<List<UserDTO>> GetTaskMembers(int taskId, CancellationToken ct)
        {
            if (taskId == 0) throw new ArgumentNullException("Task Id is null", nameof(taskId));
            try
            {
                var taskMembers = await _tasksRepository.GetTaskMembers(taskId, ct);
                var mapped = taskMembers.Select(t => new UserDTO
                {
                    Id = t.UserId,
                    FullName = t.User != null ? t.User.FullName : null,
                    Email = t.User != null ? t.User.Email : null,
                }).ToList();
                return mapped;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GetTaskMembers was cancelled. TaskId: {TaskId}", taskId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching members!");
                return new List<UserDTO>();
            }
        }
        public async Task<ResponseDTO> RemoveMember(string id, int taskId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            try
            {
                var result = await _tasksRepository.RemoveMember(id, taskId, ct);
                if (result == null)
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Error while removing member."
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Member removed successfully."
                };

            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("RemoveMember was cancelled. MemberId: {MemberId}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member!");
                return new ResponseDTO
                {
                    Success = false,
                    Message = "Error removing member!"
                };
            }
        }
        public async Task<ResponseDTO> SaveJiraWebhookTask(int projectId, JiraWebhookPayload payload, CancellationToken ct)
        {
            try
            {
                if (payload?.Issue == null)
                    return new ResponseDTO { Success = false, Message = "Invalid payload" };

                var issueKey = payload.Issue.Key;

                var existing = await _tasksRepository.GetTaskByExternalIdAsync(issueKey, ct);

                if (payload.WebhookEvent == "jira:issue_created")
                {
                    if (existing != null)
                    {
                        return new ResponseDTO
                        {
                            Success = true,
                            Message = "Task already exists"
                        };
                    }

                    var task = new TA_Domain.Entities.Task
                    {
                        ProjectId = projectId,
                        ExternalId = issueKey,
                        Title = payload.Issue.Fields.Summary,
                        Description = payload.Issue.Fields.Description,
                        Status = payload.Issue.Fields.Status?.Name,
                        ImportStatus = payload.Issue.Fields.Status?.Name,
                        Priority = payload.Issue.Fields.Priority?.Name
                    };

                    await _tasksRepository.CreateTaskAsync(task, ct);

                    return new ResponseDTO
                    {
                        Success = true,
                        Message = "Task created successfully"
                    };
                }

                else if (payload.WebhookEvent == "jira:issue_updated")
                {
                    if (existing == null)
                    {
                        return new ResponseDTO
                        {
                            Success = false,
                            Message = "Task not found for update"
                        };
                    }
                   
                    var updatedTask = new TA_Domain.Entities.Task
                    {
                        TaskId = existing.TaskId,
                        Title = payload.Issue.Fields.Summary,
                        Description = payload.Issue.Fields.Description,
                        Status = payload.Issue.Fields.Status?.Name,
                        Priority = payload.Issue.Fields.Priority?.Name
                    };

                    await _tasksRepository.UpdateTaskAsync(updatedTask, ct);

                    return new ResponseDTO
                    {
                        Success = true,
                        Message = "Task updated from Jira webhook"
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Event ignored"
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SaveJiraWebhookTask was cancelled. ProjectId: {ProjectId}", projectId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving task from Jira webhook!");
                throw;
            }
        }
        public async Task<ResponseDTO> ProcessTrelloWebhookAsync(int projectId, TrelloWebhookPayload payload, CancellationToken ct)
        {
            try
            {
                var card = payload.Action.Data.Card;

                var existingTask = await _tasksRepository.GetTaskByExternalIdAsync(card.Id, ct);

                if (existingTask != null)
                {
                    var updatedTask = new TA_Domain.Entities.Task
                    {
                        TaskId = existingTask.TaskId,
                        Title = !string.IsNullOrWhiteSpace(card.Name)
                               ? card.Name
                               : existingTask.Title,

                        Description = !string.IsNullOrWhiteSpace(card.Desc)
                               ? card.Desc
                               : existingTask.Description,

                        Status = !string.IsNullOrWhiteSpace(payload.Action.Data.List?.Name)
                               ? payload.Action.Data.List!.Name
                               : existingTask.Status
                    };


                    await _tasksRepository.UpdateTaskAsync(updatedTask, ct);

                    return new ResponseDTO
                    {
                        Success = true,
                        Message = "Task updated from Jira webhook"
                    };
                }
                else if (payload.Action.Type == "createCard")
                {
                    var newTask = new TA_Domain.Entities.Task
                    {
                        ProjectId = projectId,
                        ExternalId = card.Id,
                        Title = card.Name,
                        Description = card.Desc,
                        Status = payload.Action.Data.List?.Name,
                        ImportStatus = payload.Action.Data.List?.Name,
                        Priority = "Medium" // Trello doesn't have native priority, so this is just a placeholder
                    };

                    await _tasksRepository.CreateTaskAsync(newTask, ct);
                    return new ResponseDTO
                    {
                        Success = true,
                        Message = "Task created from Trello webhook"
                    };
                }
                return new ResponseDTO
                {
                    Success = true,
                    Message = "Event ignored"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Trello webhook!");
                return new ResponseDTO
                {
                    Success = false,
                    Message = "Error processing Trello webhook"
                };
            }
        }
    }
}
