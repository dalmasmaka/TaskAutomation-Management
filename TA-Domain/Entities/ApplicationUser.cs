using Microsoft.AspNetCore.Identity;

namespace TA_Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
