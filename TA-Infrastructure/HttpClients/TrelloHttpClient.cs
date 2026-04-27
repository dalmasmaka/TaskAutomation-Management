using TA_Application.DTOs;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using TA_Application.Configurations;
using TA_Application.Interfaces.Services;
namespace TA_Infrastructure.HttpClients
{
    public class TrelloHttpClient : ITrelloHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly TaskProvidersSettings _settings;

        public TrelloHttpClient(HttpClient httpClient, IOptions<ProvidersSettings> settings)
        {
            _httpClient = httpClient;

            _settings = settings.Value.Trello
                ?? throw new ArgumentNullException("Trello settings are missing");
        }

        public Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, int projectId, int companyId)
        {
            var frontendUrl = "http://localhost:4200/";
            var returnUrl = $"{frontendUrl}/integration/callback?projectId={projectId}&companyId={companyId}";

            var url = $"https://trello.com/1/authorize" +
                      $"?key={clientId}" +
                      $"&response_type=token" +
                      $"&scope=read,write" +
                      $"&expiration=never" +
                      $"&return_url={Uri.EscapeDataString(returnUrl)}";

            return System.Threading.Tasks.Task.FromResult(url);
        }

        public async Task<List<Board>> GetBoardsAsync(string key, string accessToken)
        {

            var url = $"https://api.trello.com/1/members/me/boards" +
                      $"?key={key}&token={accessToken}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Trello API error: {response.StatusCode} - {error}");
            }

            var boards = await response.Content.ReadFromJsonAsync<List<Board>>();

            return boards ?? [];
        }

        public async Task<List<TaskDTO>> GetTrelloTasks(string accessToken, string boardId, string apiKey, int projectId)
        {
            var url = $"https://api.trello.com/1/boards/{boardId}/cards" +
              $"?key={apiKey}&token={accessToken}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Trello API error: {response.StatusCode} - {error}");
            }

            var trelloCards = await response.Content.ReadFromJsonAsync<List<TrelloCardResponse>>();

            return trelloCards!.Select(card => new TaskDTO
            {
                ExternalId = card.Id,
                Title = card.Name,
                Description = card.Desc,
                ProjectId = projectId,
                ImportStatus = card.List?.Name ?? "Unknown",
                Status = card.List?.Name ?? "Unknown",
            }).ToList();

        }
    }
}
