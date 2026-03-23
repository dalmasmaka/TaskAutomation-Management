using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class ApplicationUserRepository : IApplicationUserRepository
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

        public async Task<ApplicationUser> GetByUsernameAsync(string username)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);
        }
    }
}
