using Microsoft.EntityFrameworkCore;
using Octokit;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;
using TA_Infrastructure.Persistence.Migrations;

namespace TA_Infrastructure.Repositories
{
    public class ApplicationUserRepository : IAppUserRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ApplicationUser> AddAsync(ApplicationUser user, CancellationToken ct)
        {
            var result = await _context.Users.AddAsync(user, ct);

            await _context.SaveChangesAsync(ct);

            return result.Entity;
        }

        public async Task<bool> ChangePassword(string userId, string newPassword, CancellationToken ct)
        {
            var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null)
                return false;
            user.PasswordHash = newPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
            return true;

        }

        public async Task<string> GetPasswordByUserIdAsync(string userId, CancellationToken ct)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PasswordHash)
                .FirstOrDefaultAsync(ct);

            ArgumentNullException.ThrowIfNull(user);

            return user;
        }

        public async Task<bool> DeleteUser(string id, CancellationToken ct)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null)
                return false;
            existing.isDeleted = true;

            _context.Users.Update(existing);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsers(CancellationToken ct)
        {
            return await _context.Users.ToListAsync(ct);
        }

        public async Task<ApplicationUser> GetByEmailAsync(string email, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(email);

            return await _context.Users
         .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<int> GetTotalUsersCountAsync(CancellationToken ct)
        {
            return await _context.Users.CountAsync(ct);
        }

        public async Task<ApplicationUser> GetUserById(string id, CancellationToken ct)
        {
            var users = await _context.Users.FindAsync(id);
            ArgumentNullException.ThrowIfNull(users);
            return users;
        }

        public async Task<ApplicationUser> UpdateUser(ApplicationUser user, CancellationToken ct)
        {
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUsersByCompanyIdAsync(int companyId, CancellationToken ct)
        {
            var users = await _context.Users
                .Where(u => u.CompanyId == companyId && !u.isDeleted)
                .ToListAsync(ct);

            if (!users.Any())
                return false;

            foreach (var user in users)
            {
                user.isDeleted = true;
            }

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<TA_Domain.Entities.UserGitHubConfig> GetUserGitHubInfo(string userId, CancellationToken ct)
        {
            return await _context.UserGitHubConfigs.Where(g => g.UserId == userId).FirstOrDefaultAsync(ct);
        }

        public async Task<TA_Domain.Entities.UserGitHubConfig> SaveUserGitHubInfo(TA_Domain.Entities.UserGitHubConfig userGitHub, CancellationToken ct)
        {
            if (userGitHub.Id == 0)
            {
                await _context.UserGitHubConfigs.AddAsync(userGitHub, ct);
            }
            else
            {
                var existing = await _context.UserGitHubConfigs
                    .FirstOrDefaultAsync(x => x.Id == userGitHub.Id, ct);

                if (existing == null)
                    throw new KeyNotFoundException("UserGitHubConfig not found");

                existing.GitHubUsername = userGitHub.GitHubUsername;
                existing.AccessToken = userGitHub.AccessToken;
            }

            await _context.SaveChangesAsync(ct);
            return userGitHub;
        }
    }
}
