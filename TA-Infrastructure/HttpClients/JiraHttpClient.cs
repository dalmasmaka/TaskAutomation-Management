using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using TA_Application.Configurations;
using TA_Application.DTOs;
using System.Net.Http.Headers;
using TA_Application.Interfaces.Services;
namespace TA_Infrastructure.HttpClients
{
    public class JiraHttpClient : IJiraHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly TaskProvidersSettings _settings;

        public JiraHttpClient(HttpClient httpClient, IOptions<TaskProvidersSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        // 🔹 1. Authorization URL
        public Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri)
        {
            var url = $"{_settings.AuthBaseUrl}{_settings.AuthorizeEndpoint}" +
                      $"?audience=api.atlassian.com" +
                      $"&client_id={clientId}" +
                      $"&scope=read:jira-work" +
                      $"&redirect_uri={redirectUri}" +
                      $"&response_type=code" +
                      $"&prompt=consent";

            return Task.FromResult(url);
        }
        // 🔹 2. Exchange Code → Access Token
        public async Task<string> ExchangeCodeForTokenAsync(
            string code,
            int porjectId,
            string clientId,
            string clientSecret,
            string redirectUri)
        {
            var tokenUrl = $"{_settings.AuthBaseUrl}{_settings.TokenEndpoint}";

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = JsonContent.Create(new
                {
                    grant_type = "authorization_code",
                    client_id = clientId,
                    client_secret = clientSecret,
                    code = code,
                    redirect_uri = redirectUri
                })
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("access_token").GetString()!;
        }
        // 🔹 3. Get CloudId
        public async Task<string> GetCloudIdAsync(string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_settings.ApiBaseUrl}/oauth/token/accessible-resources");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement[0].GetProperty("id").GetString()!;
        }
        // 🔹 4. Get Tasks
        public async Task<TaskDTO[]> GetTasksAsync(string accessToken, string cloudId)
        {
            var url = $"{_settings.ApiBaseUrl}/ex/jira/{cloudId}/rest/api/3/search";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var issues = doc.RootElement.GetProperty("issues");

            var tasks = new List<TaskDTO>();

            foreach (var issue in issues.EnumerateArray())
            {
                var fields = issue.GetProperty("fields");

                tasks.Add(new TaskDTO
                {
                    ExternalId = issue.GetProperty("id").GetString()!,
                    Title = fields.GetProperty("summary").GetString()!,
                    Description = fields.TryGetProperty("description", out var desc)
                        ? desc.ToString()
                        : null,
                    Status = fields.GetProperty("status")
                                  .GetProperty("name")
                                  .GetString()!,
                });
            }

            return tasks.ToArray();
        }
    }
}
