using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class JiraProvider : ITaskProviderService
    {
        private readonly IJiraHttpClient _httpClient;
        private readonly IProjectIntegrationRepository _projectRepo;
        private readonly CompanyIntegrationConfig _config;
        private readonly WebhookServiceFactory _webhookFactory;

        public JiraProvider(IJiraHttpClient httpClient, IProjectIntegrationRepository projectRepo, CompanyIntegrationConfig config, WebhookServiceFactory webhookFactory)
        {
            _httpClient = httpClient;
            _projectRepo = projectRepo;
            _config = config;
            _webhookFactory = webhookFactory;
        }
        public async Task<string> GetAuthUrlAsync(int projectId, int companyId)
            => await _httpClient.GetAuthorizationUrlAsync(_config.ClientId!, _config.RedirectUri!, projectId, companyId);

        public async System.Threading.Tasks.Task AuthorizeAndSaveAsync(string code, int projectId)
        {
            // 1. Exchange the temporary code for a long-lived Access Token
            var accessToken = await _httpClient.ExchangeCodeForTokenAsync(
                code, projectId, _config.ClientId!, _config.ClientSecret!, _config.RedirectUri!);

            // 2. Get the specific "Cloud ID" (ExternalId) for this Jira instance
            var cloudId = await _httpClient.GetCloudIdAsync(accessToken.AccessToken);
            
            // 3. Save or Update the ProjectIntegration record
            var integration = new ProjectIntegrationDTO
            {
                ProjectId = projectId,
                Provider = "Jira",
                Project = null!, // This will be set by the repository when linking to the actual Project entity
                AccessToken = accessToken.AccessToken,
                RefreshToken = accessToken.RefreshToken,
                CloudId = cloudId,
                ExpiresAt = DateTime.UtcNow.AddHours(1) // Jira tokens usually last 1 hour
            };

            await _projectRepo.SaveAsync(integration);
         
           
        }
        public async Task<List<Board>> GetBoardsAsync(int projectId)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId);

            if (integration == null || string.IsNullOrEmpty(integration.AccessToken))
                throw new Exception("Project is not integrated with Jira.");

            return await _httpClient.GetBoardsAsync(integration.AccessToken, integration.CloudId!);
        }
        public async System.Threading.Tasks.Task SaveBoardSelectionAsync(int projectId, string boardId, string key)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId);

            if (integration == null)
                throw new Exception("Integration record not found.");

            // Update the ExternalBoardId (Ensure this property exists in your DTO/Entity)
            integration.ExternalBoardId = boardId;
            integration.ExternalId = key;
            await _projectRepo.SaveAsync(integration);
            var result = await _httpClient.GetJiraTasksAsync(
                   integration.AccessToken!,
                   integration.CloudId!,
                   key,
                   projectId
               );

            await _projectRepo.SaveTasksAsync(result);
            var webhookService = await _webhookFactory.GetAsync("Jira", integration.CompanyId);
            await webhookService.RegisterAsync(integration);
        }

        public async Task<ResponseDTO> SaveTasks(int projectId)
        {
            try
            {
                var integration = await _projectRepo.GetByProjectIdAsync(projectId);
                if (integration == null)
                {
                    return new ResponseDTO { Success = false, Message = "Integration not found." };
                }

                var result = await _httpClient.GetJiraTasksAsync(
                  integration.AccessToken!,
                  integration.CloudId!,
                  integration.ExternalBoardId!,
                  projectId
              );
               
                await _projectRepo.SaveTasksAsync(result);
                

                return new ResponseDTO
                {
                    Success = true,
                    Message = $"Successfully imported {result.Count} tasks from Jira.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
        }
    }
}
