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
        public required string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public ApplicationUser? User { get; set; }
        // Employees of the company
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<WorkspaceInvitations>? WorkspaceInvitations { get; set; }
        public ICollection<Project>? Projects { get; set; }

    }
}
