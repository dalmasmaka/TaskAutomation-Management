using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Services
{
    public interface IGitHubService
    {
        Task<PullRequestDTO> CreatePullRequestAsync(PullRequestCreationDTO dto, GitHubConfigDTO config, UserGitHubConfigDTO userConfig);
        Task<ReviewDTO> SubmitReviewAsync(ReviewRequestDTO dto, GitHubConfigDTO config);
    }
}
