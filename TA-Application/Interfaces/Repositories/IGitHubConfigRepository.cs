using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface IGitHubConfigRepository
    {
        Task<GitHubConfigDTO> GetByProjectId(int projectId);
        Task<UserGitHubConfigDTO> GetUsersGitHubConfig(string UserId);
    }
}
