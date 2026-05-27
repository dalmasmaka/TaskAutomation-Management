using Microsoft.AspNetCore.Identity;

namespace TA_Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {

        public required string FullName { get; set; }
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        public bool isDeleted { get; set; } = false;
        public ICollection<Company>? OwnedCompanies { get; set; }
        public ICollection<UserProject>? UserProjects { get; set; }
        public ICollection<TaskMember> TaskMembers { get; set; } = new List<TaskMember>();
        public ICollection<TeamMember> TeamMembers { get; set; }= new List<TeamMember>();
    }
}
