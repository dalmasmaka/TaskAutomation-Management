using TA_Application.DTOs;
using TA_Application.Interfaces;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class JiraProvider : ITaskProviderService
    {
        private readonly IJiraHttpClient _httpClient;
        private readonly IProjectIntegrationService _projectRepo;
        private readonly CompanyIntegrationConfig _config;

        public JiraProvider(IJiraHttpClient httpClient, IProjectIntegrationService projectRepo, CompanyIntegrationConfig config)
        {
            _httpClient = httpClient;
            _projectRepo = projectRepo;
            _config = config;
        }

        public async Task<string> GetAuthUrlAsync()
            => await _httpClient.GetAuthorizationUrlAsync(_config.ClientId!, _config.RedirectUri!);

        public async System.Threading.Tasks.Task AuthorizeAndSaveAsync(string code, int projectId)
        {
            // 1. Exchange the temporary code for a long-lived Access Token
            var accessToken = await _httpClient.ExchangeCodeForTokenAsync(
                code, projectId, _config.ClientId!, _config.ClientSecret!, _config.RedirectUri!);

            // 2. Get the specific "Cloud ID" (ExternalId) for this Jira instance
            var cloudId = await _httpClient.GetCloudIdAsync(accessToken);

            // 3. Save or Update the ProjectIntegration record
            var integration = new ProjectIntegrationDTO
            {
                ProjectId = projectId,
                Provider = "Jira",
                Project = null!, // This will be set by the repository when linking to the actual Project entity
                AccessToken = accessToken,
                ExternalId = cloudId,
                ExpiresAt = DateTime.UtcNow.AddHours(1) // Jira tokens usually last 1 hour
            };

            await _projectRepo.SaveAsync(integration);
        }

        public async Task<TaskDTO[]> GetTasksAsync(int projectId)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId);

            return await _httpClient.GetTasksAsync(integration.AccessToken!, integration.ExternalId!);
        }
    }
}