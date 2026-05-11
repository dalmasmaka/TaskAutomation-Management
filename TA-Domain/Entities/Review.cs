using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class Review: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }
        public int PullRequestId { get; set; }

        [ForeignKey("PullRequestId")] // Explicitly link the ID to the Nav property
        public PullRequest PullRequest { get; set; } = null!;
        public string? Comment { get; set; }
        public required string Decision { get; set; } // Approved / NeedsChanges

    }
}


