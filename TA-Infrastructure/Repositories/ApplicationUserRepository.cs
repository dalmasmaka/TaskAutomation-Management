using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class ApplicationUserRepository : IAppUserRepository
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

        public async System.Threading.Tasks.Task ChangePassword(string userId, string newPassword)
        {
            var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            user.PasswordHash = newPassword;

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetPasswordByUserIdAsync(string userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PasswordHash)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new Exception("User not found");

            return user;
        }

        public async System.Threading.Tasks.Task DeleteUser(string id)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existing != null)
            {
                existing.isDeleted = true;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
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
