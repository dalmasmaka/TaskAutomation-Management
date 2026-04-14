using TA_Application.DTOs;

namespace TA_Infrastructure.HttpClients
{
    using System.Text.Json;
    using Microsoft.Extensions.Options;
    using TA_Application.Configurations;
    using TA_Application.Interfaces.Services;

    public class TrelloHttpClient : ITrelloHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly TaskProvidersSettings _settings;

        public TrelloHttpClient(HttpClient httpClient, IOptions<TaskProvidersSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        // 🔹 1. Authorization URL
        public Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri)
        {
            var url = $"{_settings.AuthBaseUrl}{_settings.AuthorizeEndpoint}" +
                      $"?key={clientId}" +
                      $"&response_type=token" +
                      $"&scope=read" +
                      $"&expiration=never" +
                      $"&return_url={redirectUri}";

            return Task.FromResult(url);
        }

        // 🔹 2. Token passthrough (comes from frontend)
        public Task<string> ExchangeTokenAsync(string token)
        {
            return Task.FromResult(token);
        }

        // 🔹 3. Get Tasks (cards)
        public async Task<TaskDTO[]> GetTasksAsync(string accessToken, string boardId, string clientId)
        {
            var url = $"{_settings.ApiBaseUrl}/boards/{boardId}/cards?key={clientId}&token={accessToken}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var cards = JsonSerializer.Deserialize<List<TrelloCard>>(json);

            return cards!.Select(card => new TaskDTO
            {
                ExternalId = card.id,
                Title = card.name,
                Description = card.desc,
                Status = card.idList
            }).ToArray();
        }
        // 🔹 Internal model (Infrastructure only)
        private class TrelloCard
        {
            public string id { get; set; } = null!;
            public string name { get; set; } = null!;
            public string desc { get; set; } = null!;
            public string idList { get; set; } = null!;
        }
    }
}
