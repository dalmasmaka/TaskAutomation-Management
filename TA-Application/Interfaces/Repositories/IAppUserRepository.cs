using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public  interface IAppUserRepository
    {
        Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken ct);
        Task<string> GetPasswordByUserIdAsync(string userId, CancellationToken ct);
        Task<bool> ChangePassword(string currentPassword, string newPassword, CancellationToken ct);
        Task<ApplicationUser> GetByEmailAsync(string email, CancellationToken ct);
        Task<IEnumerable<ApplicationUser>> GetAllUsers(CancellationToken ct);
        Task<ApplicationUser> GetUserById(string id, CancellationToken ct);
        Task<UserGitHubConfig> GetUserGitHubInfo(string userId, CancellationToken ct);
        Task<UserGitHubConfig> SaveUserGitHubInfo(UserGitHubConfig userGitHubDTO, CancellationToken ct);
        Task<ApplicationUser> UpdateUser(ApplicationUser user, CancellationToken ct);
        Task<bool> DeleteUser(string id, CancellationToken ct);
        Task<int> GetTotalUsersCountAsync(CancellationToken ct);
        Task<bool> DeleteUsersByCompanyIdAsync(int companyId, CancellationToken ct);
    }
}
