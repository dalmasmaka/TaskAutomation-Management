using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class Task: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskId { get; set; }
        [MaxLength(100)]
        public required string ExternalId { get; set; }
        [MaxLength(200)]
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string Status { get; set; } // new, in progress, generated etc.
        public string? ImportStatus { get; set;  }
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public required Project Project { get; set; }
        [ForeignKey("AssignedUser")]
        public int? AssignedTo {  get; set; }//nullable until assigned
        public ApplicationUser? AssignedToUser { get; set; }

        //nav props
        public ICollection<CodeGeneration>? CodeGenerations { get; set; }
    }
}
