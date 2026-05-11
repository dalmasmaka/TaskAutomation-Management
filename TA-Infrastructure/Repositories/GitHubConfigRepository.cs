using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TA_Infrastructure.Repositories
{
    public class GitHubConfigRepository : IGitHubConfigRepository
    {
        private readonly ApplicationDbContext _context;
        public GitHubConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<GitHubConfigDTO> GetByProjectId(int projectId)
        {
            return await _context.GitHubConfigs
                .Where(c => c.ProjectId == projectId)
                .Select(c => new GitHubConfigDTO
                {
                    ProjectId = c.ProjectId,
                    RepoName = c.RepoName,
                    RepoOwner = c.RepoOwner,
                    Provider = c.Provider,
                    DefaultBranch = c.DefaultBranch,
                    AccessToken = c.AccessToken
                })
                .FirstOrDefaultAsync();

        }
    }
}
