using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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
}
