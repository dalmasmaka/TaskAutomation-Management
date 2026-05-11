using Octokit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IPullRequestService
    {
        Task<PullRequestDTO> CreatePullRequestAsync(PullRequestCreationDTO pullRequestCreationDTO);
        Task<PagedResult<PullRequestListDTO>> GetAllPRAsync(PRQuery query, string userId, string role, int companyId);
        Task<List<ReviewDTO>> GetReviewsForPullRequestAsync(int pullRequestId);
        Task<ReviewDTO> SubmitReviewAsync(ReviewRequestDTO dto);
    }
}
