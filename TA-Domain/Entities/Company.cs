using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TA_Domain.Entities
{
    public class Company: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string Name { get; set; }
        [ForeignKey("Owner")]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
        public ICollection<Project>? Projects { get; set; }


    }
}
