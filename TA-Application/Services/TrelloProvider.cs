using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class TrelloProvider : ITaskProviderService
    {
        private readonly ITrelloHttpClient _httpClient;
        private readonly IProjectIntegrationRepository _projectRepo;
        private readonly CompanyIntegrationConfig _config;
        private readonly WebhookServiceFactory _webhookFactory;

        public TrelloProvider(ITrelloHttpClient httpClient, IProjectIntegrationRepository projectRepo, CompanyIntegrationConfig config, WebhookServiceFactory webhookFactory)
        {
            _httpClient = httpClient;
            _projectRepo = projectRepo;
            _config = config;
            _webhookFactory = webhookFactory;
        }

        public async Task<string> GetAuthUrlAsync(int projectId, int companyId)
            => await _httpClient.GetAuthorizationUrlAsync(_config.ClientId!, _config.RedirectUri!, projectId, companyId);

        public async System.Threading.Tasks.Task AuthorizeAndSaveAsync(string token, int projectId, CancellationToken ct)
        {
            var integration = new ProjectIntegration
            {
                ProjectId = projectId,
                Provider = "Trello",
                Project = null!, // This will be set by the repository when saving
                AccessToken = token,
                ExpiresAt = null // Trello tokens can be set to "never" expire
            };
            await _projectRepo.SaveAsync(integration, ct);
        }

        public async Task<List<Board>> GetBoardsAsync(int projectId, CancellationToken ct)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId, ct);

            if (integration == null || string.IsNullOrWhiteSpace(integration.AccessToken))
                throw new Exception("Project is not integrated with Trello.");

            var key = _config.ClientId;
            ArgumentNullException.ThrowIfNullOrEmpty(key);

            return await _httpClient.GetBoardsAsync(key, integration.AccessToken);
        }

        public async Task<bool> SaveTasks(int projectId, CancellationToken ct)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId, ct);
            if (integration == null)
            {
                return false;
            }

            var result = await _httpClient.GetTrelloTasks(
              integration.AccessToken!,
              integration.ExternalBoardId!,
              _config.ClientId!,
              projectId
          );

            await _projectRepo.SaveTasksAsync(result, ct);
            return true;
        }

        public async System.Threading.Tasks.Task SaveBoardSelectionAsync(int projectId, string boardId, string key, CancellationToken ct)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId, ct);

            if (integration == null)
                throw new Exception("Integration record not found.");

            key = _config.ClientId
                ?? throw new Exception("ClientId is not configured.");

            integration.ExternalBoardId = boardId;

            await _projectRepo.SaveAsync(integration, ct);
            if (integration.AccessToken == null)
                throw new Exception("Token not found.");

            var result = await _httpClient.GetTrelloTasks(
                integration.AccessToken,
                boardId,
                key,
                projectId
            );
            var dto = new ProjectIntegrationDTO
            {
                ProjectId = integration.ProjectId,
                AccessToken = integration.AccessToken,
                Provider = integration.Provider,
                RefreshToken = integration.RefreshToken,
                ExternalBoardId = integration.ExternalBoardId,
                ExternalId = integration.ExternalId,
                CloudId = integration.CloudId

            };
            await _projectRepo.SaveTasksAsync(result, ct);
            var webhookService = await _webhookFactory.GetAsync("Trello", integration.Project.CompanyId);
            await webhookService.RegisterAsync(dto);
        }

    }
}
