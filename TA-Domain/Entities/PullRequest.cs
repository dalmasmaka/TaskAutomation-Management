using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class PullRequest: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PRId { get; set; }
        [ForeignKey("CodeGeneration")]
        public int CodeGenId { get; set; }
        public required CodeGeneration CodeGeneration { get; set; }
        [MaxLength(200)]
        public required string BranchName { get; set; }
        public required string Status { get; set; } // Open, Merged, Closed, ChangesRequired
                                                    // Navigation properties
        public ICollection<Review>? Reviews { get; set; }
    }
}
