using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class TrelloProvider : ITaskProviderService
    {
        private readonly ITrelloHttpClient _httpClient;
        private readonly IProjectIntegrationService _projectRepo;
        private readonly CompanyIntegrationConfig _config;

        public TrelloProvider(ITrelloHttpClient httpClient, IProjectIntegrationService projectRepo, CompanyIntegrationConfig config)
        {
            _httpClient = httpClient;
            _projectRepo = projectRepo;
            _config = config;
        }

        public async Task<string> GetAuthUrlAsync()
            => await _httpClient.GetAuthorizationUrlAsync(_config.ClientId!, _config.RedirectUri!);

        public async System.Threading.Tasks.Task AuthorizeAndSaveAsync(string token, int projectId)
        {
            var integration = new ProjectIntegrationDTO
            {
                ProjectId = projectId,
                Provider = "Trello",
                Project = null!, // This will be set by the repository when saving
                AccessToken = token,
                ExternalId = _config.ExternalWorkspaceId, // Trello Board ID
                ExpiresAt = null // Trello tokens can be set to "never" expire
            };

            await _projectRepo.SaveAsync(integration);
        }

        public async Task<TaskDTO[]> GetTasksAsync(int projectId)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId);
            return await _httpClient.GetTasksAsync(integration.AccessToken!, integration.ExternalId!, _config.ClientId!);
        }
    }
}
