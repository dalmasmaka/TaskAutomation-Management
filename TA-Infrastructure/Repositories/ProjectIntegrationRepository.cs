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
                CloudId = integration.CloudId,
                CompanyId = integration.Project!.CompanyId
            };
            return System.Threading.Tasks.Task.FromResult<ProjectIntegrationDTO?>(dto);
        }

        public async System.Threading.Tasks.Task SaveAsync(ProjectIntegrationDTO integration)
        {
            var existing = await _context.ProjectIntegrations
                .FirstOrDefaultAsync(pi =>
                    pi.ProjectId == integration.ProjectId &&
                    pi.Provider == integration.Provider);

            if (existing != null)
            {
                if (integration.AccessToken != null) existing.AccessToken = integration.AccessToken;
                if (integration.RefreshToken != null) existing.RefreshToken = integration.RefreshToken;
                if (integration.ExternalId != null) existing.ExternalId = integration.ExternalId;
                if (integration.ExpiresAt != null) existing.ExpiresAt = integration.ExpiresAt;
                if (integration.ExternalBoardId != null) existing.ExternalBoardId = integration.ExternalBoardId;
                if (integration.CloudId != null) existing.CloudId = integration.CloudId;

                //_context.ProjectIntegrations.Update(existing);
            }
            else
            {
                var projectIntegration = new ProjectIntegration
                {
                    ProjectId = integration.ProjectId,
                    Provider = integration.Provider,
                    AccessToken = integration.AccessToken,
                    RefreshToken = integration.RefreshToken,
                    ExternalId = integration.ExternalId,
                    ExpiresAt = integration.ExpiresAt,
                    ExternalBoardId = integration.ExternalBoardId,
                    CloudId = integration.CloudId
                };

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
        public async Task<List<ProjectIntegrationDTO>> GetExpiringJiraTokens()
        {
            var threshold = DateTime.UtcNow.AddMinutes(10);

            var entity=  await _context.ProjectIntegrations
                 .Include(x => x.Project)
                .Where(x => x.Provider == "Jira" &&
                            x.ExpiresAt <= threshold &&
                            x.RefreshToken != null)
                .ToListAsync();
            var dtoList = entity.Select(integration => new ProjectIntegrationDTO
            {
                Id = integration.Id,
                ProjectId = integration.ProjectId,
                Project = integration.Project,
                Provider = integration.Provider,
                AccessToken = integration.AccessToken,
                RefreshToken = integration.RefreshToken,
                CompanyId = integration.Project!.CompanyId,
            }).ToList();
            return dtoList;
        }
        public async Task<ProjectDTO> GetProjectByExternalId(string externalId)
        {
            return await _context.ProjectIntegrations
                .Where(p => !p.IsDeleted && p.ExternalBoardId == externalId)
                .Select(project => new ProjectDTO{
                    ProjectId = project.ProjectId }).FirstOrDefaultAsync();
        }
    }
}
