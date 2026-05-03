using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TA_Application.DTOs
{
    public class WebhookResponse
    {
        // The JSON returns "webhookRegistrationResult" (singular)
        [JsonPropertyName("webhookRegistrationResult")]
        public List<WebhookRegistrationResult> WebhookRegistrationResults { get; set; } = new();
    }

    public class WebhookRegistrationResult
    {
        // The JSON returns an integer (39). Change this to 'long' or 'int' to capture it.
        [JsonPropertyName("createdWebhookId")]
        public long? CreatedWebhookId { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }
    public class JiraWebhookListResponse
    {
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public int Total { get; set; }
        public bool IsLast { get; set; }

        // This list maps to the "values" array in the JSON
        public List<JiraWebhookValue> Values { get; set; } = new();
    }
    public class JiraWebhookValue
    {
        public long Id { get; set; }
        public string JqlFilter { get; set; }
        public List<string> Events { get; set; }

        // This MUST be a string now to avoid the ISO 8601 offset error (+0200 vs +02:00)
        public string ExpirationDate { get; set; }

        public string Url { get; set; }
    }
    public class TrelloWebhookResponse
    {
        public string Id { get; set; }           // The Unique Webhook ID (e.g., "6630ce...")
        public string Description { get; set; }  // The name you gave the webhook
        public string IdModel { get; set; }      // The ID of the Board/List being watched
        public string CallbackURL { get; set; }  // Your API endpoint
        public bool Active { get; set; }         // Status of the webhook
    }

}