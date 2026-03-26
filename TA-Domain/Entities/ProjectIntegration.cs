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
        public required Project Project { get; set; }

        public string? Provider { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public string? ExternalId { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
