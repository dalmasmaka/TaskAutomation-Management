using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
            var scopes = "read:jira-work read:project:jira-software read:board-scope:jira-software offline_access";
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
        public async Task<List<TaskDTO>> GetJiraTasksAsync(string accessToken, string cloudId, string projectKey, int localProjectId)
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

            var tasks = new List<TaskDTO>();

            if (doc.RootElement.TryGetProperty("issues", out var issues))
            {
                foreach (var issue in issues.EnumerateArray())
                {
                    var fields = issue.GetProperty("fields");

                    var title = fields.TryGetProperty("summary", out var summaryProp)
                        ? summaryProp.GetString()
                        : "";

                    //string description = "";
                    //if (fields.TryGetProperty("description", out var descProp))
                    //{

                    //    if (descProp.ValueKind == JsonValueKind.String)
                    //        description = descProp.GetString();
                    //    else if (descProp.ValueKind == JsonValueKind.Object)
                    //        description = ExtractTextFromAdf(descProp);
                    //}

                    //string statusName = "";
                    //if (fields.TryGetProperty("status", out var statusProp) &&
                    //    statusProp.TryGetProperty("name", out var nameProp))
                    //{
                    //    statusName = nameProp.GetString();
                    //}
                    //string priorityName = "Medium"; 
                    //if (fields.TryGetProperty("priority", out var priorityProp) &&
                    //    priorityProp.TryGetProperty("name", out var pNameProp))
                    //{
                    //    priorityName = pNameProp.GetString();
                    //}
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

                    tasks.Add(new TaskDTO
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
        private string ExtractTextFromAdf(JsonElement adfElement)
        {
            try
            {
                // Digging into: description -> content -> content -> text
                if (adfElement.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
                {
                    var firstParagraph = contentArray[0];
                    if (firstParagraph.TryGetProperty("content", out var textNodes) && textNodes.GetArrayLength() > 0)
                    {
                        return textNodes[0].GetProperty("text").GetString() ?? "";
                    }
                }
            }
            catch
            {
                // Fallback: If it's too complex, just return the raw text so you don't lose data
                return adfElement.GetRawText();
            }
            return "";
        }
    }
}
