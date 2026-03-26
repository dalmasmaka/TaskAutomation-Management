using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user);
    }
}
