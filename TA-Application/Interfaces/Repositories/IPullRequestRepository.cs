using Octokit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface IPullRequestRepository
    {
        Task<PullRequestDTO> SavePullRequestAsync(PullRequestDTO request);
        Task<ReviewDTO> SaveReviewAsync(ReviewDTO review);
        IQueryable<PullRequestListDTO> GetAllPRsAsync(string userId, string role, int companyId);
        Task<List<ReviewDTO>> GetReviewsForPullRequestAsync(int pullRequestId);
        Task<PullRequestDTO> GetPullRequestById(int id);
        Task<List<ReviewDTO>> GetWorkspaceReviews(int companyId, DateTime startDate);
        Task<ResponseDTO?> UpdatePrStatus(int pRId, string decision);
    }
}
