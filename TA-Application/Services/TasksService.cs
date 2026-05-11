using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async System.Threading.Tasks.Task DeleteAsync(int id)
        {
            try
            {
                if (id == 0) throw new ArgumentException("Id is null", nameof(id));
                await _tasksRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task!");
            }
        }


        public async Task<PagedResult<TaskDTO>> GetProjectTasks(int projectId)
        {
            var query = _tasksRepository.GetProjectTasks(projectId);

            var totalCount = await query.CountAsync();

            var items = await query.ToListAsync();

            return new PagedResult<TaskDTO>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<TaskDTO> GetTaskByIdAsync(int id)
        {
            try
            {
                if (id == 0) throw new ArgumentException("Task id is null", nameof(id));
                var task = await _tasksRepository.GetTaskById(id);
                var dto = new TaskDTO
                {
                    Title = task.Title,
                    Description = task.Description,
                    ImportStatus = task.ImportStatus,
                    Status = task.Status,
                    Priority = task.Priority,
                    //AssignedTo = task.AssignedTo
                };
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task!");
                return new TaskDTO();
            }
        }

        public async Task<TaskMembersResponseDTO> GetAllMembersAsync(int taskId)
        {
            try
            {
                var task = await _tasksRepository.GetTaskById(taskId);
                if (task == null)
                    throw new Exception("Task not found");
                var assignedMembers = await _tasksRepository.GetTaskMembers(taskId);
                var projectMembers = await _projectRepository.GetMembersAsync(task.ProjectId);
                var assignedIds = assignedMembers.Select(a => a.Id).ToHashSet();

                var availableMembers = projectMembers
                    .Where(pm => !assignedIds.Contains(pm.Id))
                    .ToList();

                return new TaskMembersResponseDTO
                {
                    AssignedMembers = assignedMembers,
                    AvailableMembers = availableMembers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching members!");
                return new TaskMembersResponseDTO();
            }
        }

        public async Task<PagedResult<TaskDTO>> GetTasksAsync(TaskQuery query, string userId, string role, int companyId)
        {
            var tasks = _tasksRepository.GetAllTasks(userId, role, companyId);

            if (query.ProjectIds?.Any() == true)
            {
                tasks = tasks.Where(t => query.ProjectIds.Contains(t.ProjectId));
            }

            if (query.CompanyId.HasValue)
                tasks = tasks.Where(x => x.CompanyId == query.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var statusList = query.Status
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());

                tasks = tasks.Where(t => statusList.Contains(t.Status));
            }

            if (query.AssignedToIds?.Any() == true)
            {
                tasks = tasks.Where(t =>
                    t.Assigners!.Any(a => query.AssignedToIds.Contains(a.Id!))
                );
            }
            if (!string.IsNullOrWhiteSpace(query.Priority))
            {
                var priorityList = query.Priority
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim());

                tasks = tasks.Where(t => priorityList.Contains(t.Priority));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
                tasks = tasks.Where(x => x.Title!.Contains(query.Search));

            var totalCount = await tasks.CountAsync();

            var data = await tasks
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<TaskDTO>
            {
                Items = data,
                TotalCount = totalCount
            };
        }

        public async Task<CompanyAssetsDTO> GetUsersAndProjectsAsync(int companyId, string role, string userId)
        {
            try
            {
                List<ProjectDTO> projects = new();
                List<UserDTO> users = new();

                if (role == "Owner")
                {
                    // Get all company projects
                    projects = await _projectRepository.GetAllAsync(companyId, userId);

                    // Get all users in company
                    users = await _companyRepository.GetTeamOfCompany(companyId);
                }
                else if (role == "Developer")
                {
                    // Get only projects where developer is assigned
                    projects = await _projectRepository.GetProjectsByUserIdAsync(companyId, userId);

                    //// Optional: only return the current developer OR teammates in those projects
                    //users = await _companyRepository.GetUsersByProjectIdsAsync(
                    //    projects.Select(p => p.ProjectId).ToList()
                    //);
                }

                return new CompanyAssetsDTO
                {
                    Projects = projects,
                    Assigners = users
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching members!");
                return new CompanyAssetsDTO();
            }
        }
        public async Task<UpdateTaskDTO> UpdateAsync(UpdateTaskDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Task is null");

            var task = await _tasksRepository.UpdateTaskAsync(dto);
            if (dto.Members != null && dto.Members.Any())
            {
                foreach (var userId in dto.Members)
                {
                    var memberDto = new TaskMembersDTO
                    {
                        TaskId = dto.TaskId,
                        UserId = userId
                    };

                    await _tasksRepository.SaveTaskMembers(memberDto);
                }
            }

            return task;
        }
        public async Task<List<UserDTO>> GetTaskMembers(int taskId)
        {
            if (taskId == 0) throw new ArgumentNullException("Task Id is null", nameof(taskId));
            try
            {
                var taskMembers = await _tasksRepository.GetTaskMembers(taskId);
                return taskMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching members!");
                return new List<UserDTO>();
            }
        }
        public async System.Threading.Tasks.Task RemoveMember(string id, int taskId)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            try
            {
                await _tasksRepository.RemoveMember(id, taskId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member!");
            }
        }
        public async Task<ResponseDTO> SaveJiraWebhookTask(int projectId, JiraWebhookPayload payload)
        {
            try
            {
                if (payload?.Issue == null)
                    return new ResponseDTO { Success = false, Message = "Invalid payload" };

                var issueKey = payload.Issue.Key;

                var existing = await _tasksRepository.GetTaskByExternalIdAsync(issueKey);

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

                    var task = new TaskDTO
                    {
                        ProjectId = projectId,
                        ExternalId = issueKey,
                        Title = payload.Issue.Fields.Summary,
                        Description = payload.Issue.Fields.Description,
                        Status = payload.Issue.Fields.Status?.Name,
                        ImportStatus = payload.Issue.Fields.Status?.Name,
                        Priority = payload.Issue.Fields.Priority?.Name
                    };

                    await _tasksRepository.CreateTaskAsync(task);

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

                    //// Optional: only update if status actually changed
                    //var statusChanged = payload.Changelog?.Items?
                    //    .Any(x => x.Field == "status") == true;

                    var updatedTask = new UpdateTaskDTO
                    {
                        TaskId = existing.TaskId,
                        Title = payload.Issue.Fields.Summary,
                        Description = payload.Issue.Fields.Description,
                        Status = payload.Issue.Fields.Status?.Name,
                        Priority = payload.Issue.Fields.Priority?.Name
                    };

                    // 🔥 THIS was missing
                    await _tasksRepository.UpdateTaskAsync(updatedTask);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving task from Jira webhook!");
                throw;
            }
        }

        public async Task<ResponseDTO> ProcessTrelloWebhookAsync(int projectId, TrelloWebhookPayload payload)
        {
            try
            {
                var card = payload.Action.Data.Card;

                var existingTask = await _tasksRepository.GetTaskByExternalIdAsync(card.Id);

                if (existingTask != null)
                {
                    var updatedTask = new UpdateTaskDTO
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


                    await _tasksRepository.UpdateTaskAsync(updatedTask);

                    return new ResponseDTO
                    {
                        Success = true,
                        Message = "Task updated from Jira webhook"
                    };
                }
                else if (payload.Action.Type == "createCard")
                {
                    var newTask = new TaskDTO
                    {
                        ProjectId = projectId,
                        ExternalId = card.Id, 
                        Title = card.Name,
                        Description = card.Desc,
                        Status = payload.Action.Data.List?.Name,
                        ImportStatus = payload.Action.Data.List?.Name,
                        Priority = "Medium" // Trello doesn't have native priority, so this is just a placeholder
                    };

                    await _tasksRepository.CreateTaskAsync(newTask);
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
