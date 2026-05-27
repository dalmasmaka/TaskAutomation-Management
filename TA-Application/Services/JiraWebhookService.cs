using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class JiraWebhookService : IWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly IWebhookRepository _webhookRepo;
        private CompanyIntegrationConfig _config;
        private readonly JsonSerializerOptions _jsonOptions;
        public JiraWebhookService(HttpClient httpClient, IWebhookRepository webhookRepo)
        {
            _httpClient = httpClient;
            _webhookRepo = webhookRepo;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        /// <summary>
        /// Retrieves all webhooks from Jira and maps them to your JiraWebhookListResponse DTO.
        /// </summary>
        public async Task<JiraWebhookListResponse> GetAllWebhooksAsync(string cloudId, string accessToken)
        {
            var url = $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3/webhook";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new JiraWebhookListResponse();

            return await response.Content.ReadFromJsonAsync<JiraWebhookListResponse>(_jsonOptions)
                   ?? new JiraWebhookListResponse();
        }

        /// <summary>
        /// Deletes a specific webhook from Jira by its ID.
        /// </summary>
        public async Task<bool> DeleteWebhookAsync(string cloudId, string accessToken, long webhookId)
        {
            var url = $"https://api.atlassian.com/ex/jira/{cloudId}/rest/api/3/webhook";

            // Jira expects: { "webhookIds": [ id1, id2 ] }
            var deletePayload = new { webhookIds = new[] { webhookId } };

            using var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                Content = JsonContent.Create(deletePayload)
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        public System.Threading.Tasks.Task HandleAsync(string provider, object payload)
        {
            throw new NotImplementedException();
        }
        public void Initialize(CompanyIntegrationConfig config)
        {
            _config = config;
        }
        public async System.Threading.Tasks.Task RegisterAsync(ProjectIntegrationDTO integration)
        {
            var registrationUrl = $"https://api.atlassian.com/ex/jira/{integration.CloudId}/rest/api/3/webhook";

            var targetUrl = "https://oblivion-fiddle-bunion.ngrok-free.dev/api/webhooks/jira/receive";

            using (var getRequest = new HttpRequestMessage(HttpMethod.Get, registrationUrl))
            {
                getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", integration.AccessToken);
                var getResponse = await _httpClient.SendAsync(getRequest);

                if (getResponse.IsSuccessStatusCode)
                {
                    var existingHooks = await getResponse.Content.ReadFromJsonAsync<JiraWebhookListResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var existing = existingHooks?.Values?.FirstOrDefault(x => x.Url.Equals(targetUrl, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        await _webhookRepo.SaveLogAsync(new ProjectWebhook
                        {
                            ExternalWebhookId = existing.Id.ToString(),
                            ProjectId = integration.ProjectId,
                            Provider = "Jira",
                            CallbackUrl = targetUrl
                        });

                        return;
                    }
                }
            }

            // 2. Create the SINGLE global webhook
            var webhookData = new
            {
                name = "Global TaskAutomation Webhook",
                url = targetUrl,
                webhooks = new[]
                {
            new {
                jqlFilter = "issuetype IN (Task, Bug, Story, Epic, Sub-task)",
                events = new[] { "jira:issue_created", "jira:issue_updated" }
            }
        }
            };

            using var postRequest = new HttpRequestMessage(HttpMethod.Post, registrationUrl);
            postRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", integration.AccessToken);
            postRequest.Content = JsonContent.Create(webhookData);

            var response = await _httpClient.SendAsync(postRequest);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadFromJsonAsync<WebhookResponse>(
     new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Extract the numeric ID and convert to string
                var webhookId = result?.WebhookRegistrationResults?.FirstOrDefault()?.CreatedWebhookId?.ToString();

                if (!string.IsNullOrEmpty(webhookId))
                {
                    await _webhookRepo.SaveLogAsync(new ProjectWebhook
                    {
                        ExternalWebhookId = webhookId,
                        ProjectId = integration.ProjectId,
                        Provider = "Jira",
                        CallbackUrl = targetUrl
                    });
                }
            }
        }
    }
}
