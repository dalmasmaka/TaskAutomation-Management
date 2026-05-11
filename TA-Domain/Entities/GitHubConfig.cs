using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TA_Domain.Entities
{
    public class GitHubConfig: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("Project")]
        public int ProjectId { get; set; } 
        public Project Project { get; set; }

        public string Provider { get; set; } // "GitHub", "GitLab"
        public string RepoOwner { get; set; }
        public string RepoName { get; set; }
        public string DefaultBranch { get; set; } = "main";

        public string AccessToken { get; set; } // or store securely
    }
}
