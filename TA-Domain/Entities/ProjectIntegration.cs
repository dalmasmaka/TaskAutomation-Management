using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class ProjectIntegration : AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public  Project? Project { get; set; }

        public string? Provider { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? CloudId { get; set; } // For Jira, this will store the "Cloud ID" which is needed for API calls. For Trello, this can be null.
        public string? ExternalId { get; set; } // For Jira this is Project Key and for Trello this is board name in our case will be null because we will be using ExternalBoardId instead
        public string? ExternalBoardId { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
