using TA_Domain.Entities;

namespace TA_Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, string role);
    }
}
