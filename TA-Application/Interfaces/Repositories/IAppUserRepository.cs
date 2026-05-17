using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public  interface IAppUserRepository
    {
        System.Threading.Tasks.Task AddAsync(ApplicationUser user);
        Task<string> GetPasswordByUserIdAsync(string userId);
        System.Threading.Tasks.Task ChangePassword(string currentPassword, string newPassword);
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task<IEnumerable<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetUserById(string id);
        Task<UserGitHubDTO> GetUserGitHubInfo(string userId);
        Task<UserGitHubDTO> SaveUserGitHubInfo(UserGitHubDTO userGitHubDTO);
        Task<ApplicationUser> UpdateUser(ApplicationUser user);
        System.Threading.Tasks.Task DeleteUser(string id);
        Task<int> GetTotalUsersCountAsync();
        System.Threading.Tasks.Task DeleteUsersByCompanyIdAsync(int companyId);
    }
}
