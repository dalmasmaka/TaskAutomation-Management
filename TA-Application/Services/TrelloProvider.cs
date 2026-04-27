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

        public TrelloProvider(ITrelloHttpClient httpClient, IProjectIntegrationRepository projectRepo, CompanyIntegrationConfig config)
        {
            _httpClient = httpClient;
            _projectRepo = projectRepo;
            _config = config;
        }

        public async Task<string> GetAuthUrlAsync(int projectId, int companyId)
            => await _httpClient.GetAuthorizationUrlAsync(_config.ClientId!, _config.RedirectUri!, projectId, companyId);

        public async System.Threading.Tasks.Task AuthorizeAndSaveAsync(string token, int projectId)
        {
            var integration = new ProjectIntegrationDTO
            {
                ProjectId = projectId,
                Provider = "Trello",
                Project = null!, // This will be set by the repository when saving
                AccessToken = token,
                ExpiresAt = null // Trello tokens can be set to "never" expire
            };

            await _projectRepo.SaveAsync(integration);
        }

        public async Task<List<Board>> GetBoardsAsync(int projectId)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId);

            if (integration == null || string.IsNullOrWhiteSpace(integration.AccessToken))
                throw new Exception("Project is not integrated with Trello.");

            var key = _config.ClientId;
            ArgumentNullException.ThrowIfNullOrEmpty(key);

            return await _httpClient.GetBoardsAsync(key, integration.AccessToken);
        }

        public async Task<ResponseDTO> SaveTasks(int projectId)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId);
            if (integration == null)
            {
                return new ResponseDTO { Success = false, Message = "Integration not found." };
            }

            var result = await _httpClient.GetTrelloTasks(
              integration.AccessToken!,
              integration.ExternalBoardId!,
              _config.ClientId!,
              projectId
          );

            await _projectRepo.SaveTasksAsync(result);

            return new ResponseDTO
            {
                Success = true,
                Message = $"Successfully imported {result.Count} tasks from Jira.",
            };
        }

        public async System.Threading.Tasks.Task SaveBoardSelectionAsync(int projectId, string boardId, string key)
        {
            var integration = await _projectRepo.GetByProjectIdAsync(projectId);

            if (integration == null)
                throw new Exception("Integration record not found.");

            key = _config.ClientId
                ?? throw new Exception("ClientId is not configured.");

            integration.ExternalBoardId = boardId;

            await _projectRepo.SaveAsync(integration);
            if (integration.AccessToken == null)
                throw new Exception("Token not found.");
            var result = await _httpClient.GetTrelloTasks(
                integration.AccessToken,
                boardId,
                key,
                projectId
            );

            await _projectRepo.SaveTasksAsync(result);
        }

    }
}
