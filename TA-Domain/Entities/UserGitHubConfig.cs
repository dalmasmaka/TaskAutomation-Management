using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TA_Domain.Entities
{
    public class UserGitHubConfig: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("UserId")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string GitHubUsername { get; set; }
        public string AccessToken { get; set; }
        //public DateTime ConnectedAt { get; set; }
        //public bool IsActive { get; set; }
        //public string GitHubUserId { get; set; }
        //public string AvatarUrl { get; set; }
        //public DateTime? TokenExpiresAt { get; set; }
    }
}
