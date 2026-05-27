using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TA_Application.Configurations;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;
namespace TA_Infrastructure.HttpClients
{
    public class JiraHttpClient : IJiraHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly TaskProvidersSettings _settings;

        public JiraHttpClient(HttpClient httpClient, IOptions<ProvidersSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value.Jira
                ?? throw new ArgumentNullException("Jira settings are missing");
        }

        // 🔹 1. Authorization URL
        public Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, int projectId, int companyId)
        {
            var state = $"{projectId}:{companyId}";
            var scopes = "read:jira-work read:project:jira-software read:board-scope:jira-software manage:jira-webhook delete:webhook:jira manage:jira-configuration read:webhook:jira write:webhook:jira offline_access";
            var url = $"{_settings.AuthBaseUrl}{_settings.AuthorizeEndpoint}" +
                      $"?audience=api.atlassian.com" +
                      $"&client_id={clientId}" +
                      $"&scope={Uri.EscapeDataString(scopes)}" +
                      $"&redirect_uri={redirectUri}" +
                      $"&state={Uri.EscapeDataString(state)}" +
                      $"&response_type=code" +
                      $"&prompt=consent";

            return System.Threading.Tasks.Task.FromResult(url);
        }
        public async Task<JiraTokenResponseDTO> ExchangeCodeForTokenAsync(string code, int projectId, string clientId, string clientSecret, string redirectUri)
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

            return new JiraTokenResponseDTO
            {
                AccessToken = doc.RootElement.GetProperty("access_token").GetString()!,
                RefreshToken = doc.RootElement.GetProperty("refresh_token").GetString()!, // ✅ THIS WAS MISSING
                ExpiresIn = doc.RootElement.GetProperty("expires_in").GetInt32()
            };
        }
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
        public async Task<List<Board>> GetBoardsAsync(string accessToken, string cloudId)
        {
            var url = $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3/project/search";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"401 Unauthorized: {errorDetail}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var boardList = new List<Board>();
            if (doc.RootElement.TryGetProperty("values", out var values))
            {
                foreach (var board in values.EnumerateArray())
                {
                    boardList.Add(new Board
                    {
                        Id = board.GetProperty("id").GetString()!,
                        Name = board.GetProperty("name").GetString()!,
                        Key = board.GetProperty("key").GetString()!,
                        // CRITICAL: The JSON uses 'projectTypeKey', not 'type'
                        // We use TryGetProperty to avoid crashes if it's missing
                        Type = board.TryGetProperty("projectTypeKey", out var typeProp)
                               ? typeProp.GetString()!
                               : "software"
                    });
                }
            }

            return boardList;
        }
        public async Task<List<TA_Domain.Entities.Task>> GetJiraTasksAsync(string accessToken, string cloudId, string projectKey, int localProjectId)
        {
            string jql = $"project = '{projectKey}'";
            string encodedJql = Uri.EscapeDataString(jql);

            var fieldsToRequest = "summary,description,status";
            var url = $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3/search/jql?jql={encodedJql}&fields={fieldsToRequest}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var tasks = new List<TA_Domain.Entities.Task>();

            if (doc.RootElement.TryGetProperty("issues", out var issues))
            {
                foreach (var issue in issues.EnumerateArray())
                {
                    var fields = issue.GetProperty("fields");

                    var title = fields.TryGetProperty("summary", out var summaryProp)
                        ? summaryProp.GetString()
                        : "";

                    string description = string.Empty;

                    if (fields.TryGetProperty("description", out var descProp))
                    {
                        if (descProp.ValueKind == JsonValueKind.String)
                        {
                            description = descProp.GetString() ?? string.Empty;
                        }
                        else if (descProp.ValueKind == JsonValueKind.Object)
                        {
                            description = ExtractTextFromAdf(descProp) ?? string.Empty;
                        }
                    }

                    string statusName = string.Empty;

                    if (fields.TryGetProperty("status", out var statusProp) &&
                        statusProp.TryGetProperty("name", out var nameProp))
                    {
                        statusName = nameProp.GetString() ?? string.Empty;
                    }

                    string priorityName = "Medium";

                    if (fields.TryGetProperty("priority", out var priorityProp) &&
                        priorityProp.TryGetProperty("name", out var pNameProp))
                    {
                        priorityName = pNameProp.GetString() ?? "Medium";
                    }
                    var externalId = issue.TryGetProperty("key", out var keyProp)
                        ? keyProp.GetString()
                        : "";

                    tasks.Add(new TA_Domain.Entities.Task
                    {
                        ExternalId = externalId,
                        Title = title,
                        Description = description,
                        ImportStatus = statusName,
                        ProjectId = localProjectId,
                        Priority = priorityName,
                        Status = statusName
                    });
                }
            }

            return tasks;
        }
        private string ExtractTextFromAdf(JsonElement root)
        {
            var sb = new StringBuilder();

            void Traverse(JsonElement node)
            {
                if (node.ValueKind != JsonValueKind.Object)
                    return;

                string? type = null;

                if (node.TryGetProperty("type", out var typeProp))
                {
                    type = typeProp.GetString();
                }

                // Extract text nodes
                if (type == "text" &&
                    node.TryGetProperty("text", out var textProp))
                {
                    sb.Append(textProp.GetString());
                }

                // Add formatting
                if (type == "paragraph")
                {
                    sb.AppendLine();
                }

                if (type == "listItem")
                {
                    sb.Append("- ");
                }

                // Recursively process children
                if (node.TryGetProperty("content", out var contentProp) &&
                    contentProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var child in contentProp.EnumerateArray())
                    {
                        Traverse(child);
                    }
                }
            }

            Traverse(root);

            return sb.ToString().Trim();
        }
     
        public async Task<JiraTokenResponseDTO> RefreshTokenAsync(string refreshToken, string clientId,  string clientSecret)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://auth.atlassian.com/oauth/token");

            var body = new
            {
                grant_type = "refresh_token",
                client_id = clientId,
                client_secret = clientSecret,
                refresh_token = refreshToken
            };

            request.Content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to refresh Jira token");

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<JiraTokenResponseDTO>(content)!;
        }
       
    }
}
