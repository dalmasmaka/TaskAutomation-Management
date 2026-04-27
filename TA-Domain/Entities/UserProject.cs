using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TA_Domain.Entities
{
    public class UserProject: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("UserId")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        [ForeignKey("ProjectId")]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public string? Role { get; set; }  
    }
}
