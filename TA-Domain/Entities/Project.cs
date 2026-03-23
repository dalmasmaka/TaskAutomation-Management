using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class Project: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProjectId { get; set; }
        [Required, MaxLength(200)]
        public required string Name { get; set; }
        [MaxLength(50)]
        public required string Platform { get; set; }
        [MaxLength(500)]
        public string? RepoURl { get; set; }
        public ICollection<Task>? Tasks { get; set; }

    }
}
