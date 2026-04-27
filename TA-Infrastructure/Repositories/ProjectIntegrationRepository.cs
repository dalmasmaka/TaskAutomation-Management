using Microsoft.EntityFrameworkCore;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class ProjectIntegrationRepository : IProjectIntegrationRepository
    {
        private readonly ApplicationDbContext _context;
        public ProjectIntegrationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task<ProjectIntegrationDTO?> GetByProjectIdAsync(int projectId)
        {
           var integration = _context.ProjectIntegrations
                .Include(pi => pi.Project)
                .FirstOrDefault(pi => pi.ProjectId == projectId);
            if (integration == null)
                return System.Threading.Tasks.Task.FromResult<ProjectIntegrationDTO?>(null);
            var dto = new ProjectIntegrationDTO
            {
                Id = integration.Id,
                ProjectId = integration.ProjectId,
                Project = integration.Project,
                Provider = integration.Provider,
                AccessToken = integration.AccessToken,
                RefreshToken = integration.RefreshToken,
                ExternalId = integration.ExternalId,
                ExpiresAt = integration.ExpiresAt,
            };
            return System.Threading.Tasks.Task.FromResult<ProjectIntegrationDTO?>(dto);
        }

        public async System.Threading.Tasks.Task SaveAsync(ProjectIntegrationDTO integration)
        {
            var projectIntegration = new ProjectIntegration
            {
                ProjectId = integration.ProjectId,
                Provider = integration.Provider,
                AccessToken = integration.AccessToken,
                RefreshToken = integration.RefreshToken,
                ExternalId = integration.ExternalId,
                ExpiresAt = integration.ExpiresAt
            };

            var existing = await _context.ProjectIntegrations
                .FirstOrDefaultAsync(pi => pi.Id == integration.Id);

            if (existing != null)
            {
                existing.ProjectId = projectIntegration.ProjectId;
                existing.Project = projectIntegration.Project;
                existing.Provider = projectIntegration.Provider;
                existing.AccessToken = projectIntegration.AccessToken;
                existing.RefreshToken = projectIntegration.RefreshToken;
                existing.ExternalId = projectIntegration.ExternalId;
                existing.ExpiresAt = projectIntegration.ExpiresAt;

                _context.ProjectIntegrations.Update(existing);
            }
            else
            {
                await _context.ProjectIntegrations.AddAsync(projectIntegration);
            }
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task SaveTasksAsync(List<TaskDTO> tasks)
        {
            foreach (var taskDto in tasks)
            {

                var save = new TA_Domain.Entities.Task
                {
                    ExternalId = taskDto.ExternalId!,
                    Title = taskDto.Title!,
                    Description = taskDto.Description,
                    Status = taskDto.Status!,
                    ImportStatus = taskDto.ImportStatus,
                    ProjectId = taskDto.ProjectId
                };
                _context.Tasks.Add(save);
            }
            await _context.SaveChangesAsync();
        }
    }
}
