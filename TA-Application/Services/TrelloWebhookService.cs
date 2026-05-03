using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class TrelloWebhookService : IWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly IWebhookRepository _webhookRepo;
        private CompanyIntegrationConfig _config;
        public TrelloWebhookService(HttpClient httpClient, IWebhookRepository webhookRepo)
        {
            _httpClient = httpClient;
            _webhookRepo = webhookRepo;
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
            // 1. Retrieve the company-specific Trello API Key
            var trelloKey = _config.ClientId;

            // 2. Build the URL - ensure no leading/trailing spaces in the token or key
            var registrationUrl = $"https://api.trello.com/1/webhooks?key={trelloKey?.Trim()}&token={integration.AccessToken?.Trim()}";

            var targetUrl = $"https://oblivion-fiddle-bunion.ngrok-free.dev/api/webhooks/trello/{integration.ProjectId}";

            var webhookData = new
            {
                description = $"Webhook for {integration.ExternalBoardId}",
                callbackURL = targetUrl,
                idModel = integration.ExternalBoardId,
                active = true
            };

            // 3. Use HttpRequestMessage for better control over headers
            var request = new HttpRequestMessage(HttpMethod.Post, registrationUrl);

            // CRITICAL: This header allows Trello to bypass the ngrok "Browser Warning" page.
            // Without this, Trello sees HTML instead of your API and fails validation.
            request.Headers.Add("ngrok-skip-browser-warning", "true");

            var jsonPayload = JsonSerializer.Serialize(webhookData);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // 4. Send the request
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TrelloWebhookResponse>();

                if (result?.Id != null)
                {
                    await _webhookRepo.SaveLogAsync(new ProjectWebhookDTO
                    {
                        ExternalWebhookId = result.Id,
                        ProjectId = integration.ProjectId,
                        Provider = "Trello",
                        CallbackUrl = targetUrl
                    });
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Trello Registration failed: {error}");
            }
        }
    }
}
