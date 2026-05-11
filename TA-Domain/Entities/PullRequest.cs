using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class PullRequest : AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public long ExternalId { get; set; }
        [ForeignKey("TaskId")]
        public int TaskId { get; set; }
        public Task? Task { get; set; }
        public string? BranchName { get; set; }
        public  string? PRUrl { get; set; }
        public string? PRName { get; set; }
        public  string? PRDescription { get; set; }
        public  string? Status { get; set; } // Open, Merged, Closed, ChangesRequired
        public ICollection<Review>? Reviews { get; set; }
    }
}
