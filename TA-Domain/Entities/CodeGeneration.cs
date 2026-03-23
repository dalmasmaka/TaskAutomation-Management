
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class CodeGeneration: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CodeGenId { get; set; }
        [ForeignKey("Task")]
        public required int TaskId { get; set; }
        public required Task Task { get; set; }
        public required string GeneratedCode { get; set; }
        [MaxLength(500)]
        public string? FilePath { get; set; }
        public required string Status { get; set; } // Draft, Committed, Reviewed, Approved

        public PullRequest? PullRequest { get; set; }
    }
}
