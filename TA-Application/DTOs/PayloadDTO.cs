using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TA_Application.DTOs
{
    using System.Text.Json.Serialization;

    public class JiraWebhookPayload
    {
        [JsonPropertyName("webhookEvent")]
        public string WebhookEvent { get; set; } = string.Empty;

        [JsonPropertyName("issue")]
        public JiraIssue? Issue { get; set; }
    }

    public class JiraIssue
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("fields")]
        public IssueFields? Fields { get; set; }
    }

    public class IssueFields
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("project")]
        public JiraProject? Project { get; set; }
        // --- NEW PROPERTIES ---
        [JsonPropertyName("status")]
        public JiraStatus? Status { get; set; }

        [JsonPropertyName("priority")]
        public JiraPriority? Priority { get; set; }
    }

    // Helper class for Status
    public class JiraStatus
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty; // e.g., "To Do"
    }

    // Helper class for Priority
    public class JiraPriority
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty; // e.g., "Medium"
    }

    public class JiraProject
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty; // Jira IDs are strings in JSON
    }
    public class TrelloWebhookPayload
    {
        [JsonPropertyName("action")]
        public TrelloAction Action { get; set; } = new();
    }

    public class TrelloAction
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty; // e.g., "createCard"

        [JsonPropertyName("data")]
        public TrelloData Data { get; set; } = new();
    }

    public class TrelloData
    {
        [JsonPropertyName("card")]
        public TrelloCard Card { get; set; } = new();

        [JsonPropertyName("list")]
        public TrelloListJson List { get; set; } = new();

        [JsonPropertyName("listAfter")]
        public TrelloListJson? ListAfter { get; set; } // Used during 'updateCard' move events
    }

    public class TrelloCard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;
    }


    public class TrelloListJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty; // e.g., "Doing" or "Done"
    }
}
