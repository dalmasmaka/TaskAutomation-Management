using TA_Domain.Entities;

namespace TA_Application.Interfaces
{
    public interface IJwtRepository
    {
        string GenerateToken(ApplicationUser user);
    }
}
