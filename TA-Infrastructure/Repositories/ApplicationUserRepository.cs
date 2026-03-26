using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class ApplicationUserRepository : IApplicationUserService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async System.Threading.Tasks.Task AddAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteUser(string id)
        {
            await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetByUsernameAsync(string username)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<ApplicationUser> UpdateUser(ApplicationUser user)
        {
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return user;
        }
    }
}
