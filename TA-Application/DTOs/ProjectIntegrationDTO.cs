using System.Security;
using TA_Domain.Entities;

namespace TA_Application.DTOs
{
    public class ProjectIntegrationDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int CompanyId { get; set; }
        public Project? Project { get; set; }

        public string? Provider { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public string? ExternalId { get; set; }
        public string? CloudId { get; set; }
        public string? ExternalBoardId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
    public class TrelloTokenRequest
    {
        public string Token { get; set; } = null!;
        public int ProjectId { get; set; }
        public int CompanyId { get; set; }
    }
    public class LinkBoardRequest
    {
        public required string Provider { get; set; }
        public required int ProjectId { get; set; }
        public required int CompanyId { get; set; }
        public required string BoardId { get; set; }
        public required string ProjectKey { get; set; }
    }
    public class JiraTokenResponseDTO
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int ExpiresIn { get; set; }
    }
    public class ProjectWebhookDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        // "Jira" or "Trello"
        public string Provider { get; set; }

        // The ID returned by the provider (Jira's WebhookID or Trello's ID)
        public string ExternalWebhookId { get; set; }

        // Optional: Store the specific URL Jira/Trello is hitting
        public string CallbackUrl { get; set; }

    }
}
