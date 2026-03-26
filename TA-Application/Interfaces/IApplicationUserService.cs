using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces
{
    public  interface IApplicationUserService
    {
        System.Threading.Tasks.Task AddAsync(ApplicationUser user);
        Task<ApplicationUser> GetByUsernameAsync(string username);
        Task<IEnumerable<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetUserById(string id);
        Task<ApplicationUser> UpdateUser(ApplicationUser user);
        System.Threading.Tasks.Task DeleteUser(string id);
        Task<int> GetTotalUsersCountAsync();
    }
}
