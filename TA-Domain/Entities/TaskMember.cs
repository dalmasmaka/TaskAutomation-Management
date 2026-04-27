using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TA_Domain.Entities
{
    public class TaskMember : AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string UserId { get; set; }
        public required int TaskId { get; set; }

        public ApplicationUser? User { get; set; }
        public Task? Task { get; set; }
    }
}
