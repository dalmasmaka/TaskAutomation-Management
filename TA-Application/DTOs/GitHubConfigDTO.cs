using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TA_Domain.Entities;

namespace TA_Application.DTOs
{
    public class GitHubConfigDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public string Provider { get; set; } 
        public string RepoOwner { get; set; }
        public string RepoName { get; set; }
        public string DefaultBranch { get; set; }

        public string AccessToken { get; set; } 

    }
    public class UserGitHubConfigDTO
    {
        public string? UserId { get; set; }
        public string GitHubUsername { get; set; }
        public string AccessToken { get; set; }
        public DateTime ConnectedAt { get; set; }
        public bool IsActive { get; set; }
        public string GitHubUserId { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime? TokenExpiresAt { get; set; }

    }
}
