using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class Review: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }
        [ForeignKey("PullRequest")]
        public int PRId { get; set; }
        public required PullRequest PullRequest { get; set; }
        public string? Comment { get; set; }
        public required string Decision { get; set; } // Approved / NeedsChanges

    }
}

