using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class GitHubConfigRepository : IGitHubConfigRepository
    {
        private readonly ApplicationDbContext _context;
        public GitHubConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<GitHubConfig> GetByProjectId(int projectId, CancellationToken ct)
        {
            return await _context.GitHubConfigs
                .FirstOrDefaultAsync(c => c.ProjectId == projectId && !c.IsDeleted, ct);

        }

        public async Task<UserGitHubConfig> GetUsersGitHubConfig(string userId, CancellationToken ct)
        {
            return await _context.UserGitHubConfigs
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, ct);
        }
    }
}
