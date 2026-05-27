using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces.Repositories;
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
        public async Task<TA_Domain.Entities.ProjectIntegration?> GetByProjectIdAsync(int projectId, CancellationToken ct)
        {
           return await _context.ProjectIntegrations
                .Include(pi => pi.Project)
                .FirstOrDefaultAsync(pi => pi.ProjectId == projectId, ct);
           
        }

        public async Task<TA_Domain.Entities.ProjectIntegration> SaveAsync(TA_Domain.Entities.ProjectIntegration integration, CancellationToken ct)
        {
            var existing = await _context.ProjectIntegrations
                .FirstOrDefaultAsync(pi =>
                    pi.ProjectId == integration.ProjectId &&
                    pi.Provider == integration.Provider, ct); 

            if (existing != null)
            {
                if (integration.AccessToken != null) existing.AccessToken = integration.AccessToken;
                if (integration.RefreshToken != null) existing.RefreshToken = integration.RefreshToken;
                if (integration.ExternalId != null) existing.ExternalId = integration.ExternalId;
                if (integration.ExpiresAt != null) existing.ExpiresAt = integration.ExpiresAt;
                if (integration.ExternalBoardId != null) existing.ExternalBoardId = integration.ExternalBoardId;
                if (integration.CloudId != null) existing.CloudId = integration.CloudId;

                integration = existing;
            }
            else
            {
                _context.ProjectIntegrations.Add(integration);
            }

            await _context.SaveChangesAsync(ct); 

            return integration;
        }
        public async Task<List<TA_Domain.Entities.Task>> SaveTasksAsync(List<TA_Domain.Entities.Task> tasks, CancellationToken ct)
        {
            if (tasks == null || !tasks.Any())
            {
                return new List<TA_Domain.Entities.Task>();
            }

            _context.Tasks.AddRange(tasks);

            await _context.SaveChangesAsync(ct);

            return tasks;
        }
        public async Task<List<TA_Domain.Entities.ProjectIntegration>> GetExpiringJiraTokens(CancellationToken ct)
        {
            var threshold = DateTime.UtcNow.AddMinutes(10);

            var entity = await _context.ProjectIntegrations
                 .Include(x => x.Project)
                .Where(x => x.Provider == "Jira" &&
                            x.ExpiresAt <= threshold &&
                            x.RefreshToken != null)
                .ToListAsync();
            
            return entity;
        }
        public async Task<TA_Domain.Entities.ProjectIntegration> GetProjectByExternalId(string externalId, CancellationToken ct)
        {
            return await _context.ProjectIntegrations
              .FirstOrDefaultAsync(p => !p.IsDeleted && p.ExternalBoardId == externalId, ct);
        }
    }
}
