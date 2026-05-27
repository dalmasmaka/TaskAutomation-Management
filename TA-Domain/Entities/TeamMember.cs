using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TA_Domain.Enums;
namespace TA_Domain.Entities
{
    public class TeamMember: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public MembershipStatus Status { get; set; } = MembershipStatus.PendingApproval;
    }
}
