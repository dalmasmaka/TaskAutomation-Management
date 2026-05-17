using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octokit;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class PullRequestService : IPullRequestService
    {
        private readonly IGitHubService _gitHubService;
        private readonly IGitHubConfigRepository _gitHubConfigRepository;
        private readonly IPullRequestRepository _pullRequestRepository;
        private readonly ILogger<PullRequestService> _logger;

        public PullRequestService(
            IGitHubService gitHubService,
            IGitHubConfigRepository gitHubConfigRepository,
            IPullRequestRepository pullRequestRepository,
            ILogger<PullRequestService> logger
            )
        {
            _gitHubService = gitHubService;
            _gitHubConfigRepository = gitHubConfigRepository;
            _pullRequestRepository = pullRequestRepository;
            _logger = logger;
        }

        public async Task<PullRequestDTO> CreatePullRequestAsync(PullRequestCreationDTO dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var config = await _gitHubConfigRepository.GetByProjectId(dto.ProjectId)
                ?? throw new InvalidOperationException("GitHub is not configured for this project.");

            var userConfig = await _gitHubConfigRepository.GetUsersGitHubConfig(dto.UserId)
                ?? throw new InvalidOperationException("GitHub account is not configured for this user.");

            var pr = await _gitHubService.CreatePullRequestAsync(dto, config, userConfig);

            if (pr == null)
                throw new InvalidOperationException("Failed to create pull request.");

            pr.UserId = dto.UserId;

            await _pullRequestRepository.SavePullRequestAsync(pr);

            return pr;
        }

        public async Task<PagedResult<PullRequestListDTO>> GetAllPRAsync(PRQuery query, string userId, string role, int companyId)
        {
            var pr = _pullRequestRepository.GetAllPRsAsync(userId, role, companyId);

            if (query.ProjectIds?.Any() == true)
            {
                pr = pr.Where(t => query.ProjectIds.Contains(t.ProjectId));
            }
            if (query.TaskIds?.Any() == true)
            {
                pr = pr.Where(t => query.TaskIds.Contains(t.TaskId));
            }
            if (query.CreatedByIds?.Any() == true)
            {
                pr = pr.Where(t => query.CreatedByIds.Contains(t.CreatedById));
            }
            if (!string.IsNullOrWhiteSpace(query.Search))
                pr = pr.Where(x => x.TaskId.ToString().Equals(query.Search));
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var statusList = query.Status
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());

                pr = pr.Where(t => statusList.Contains(t.Status));
            }
            var totalCount = await pr.CountAsync();

            var data = await pr
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<PullRequestListDTO>
            {
                Items = data,
                TotalCount = totalCount
            };
        }

        public async Task<List<ReviewDTO>> GetReviewsForPullRequestAsync(int pullRequestId)
        {
            if(pullRequestId <= 0)
                throw new ArgumentException("Invalid pull request ID.");

            try
            {
                var reviews = await _pullRequestRepository.GetReviewsForPullRequestAsync(pullRequestId);
                return reviews;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ReviewDTO> SubmitReviewAsync(ReviewRequestDTO dto)
        {
            // 1. Guard clause: Use the built-in throw helper
            ArgumentNullException.ThrowIfNull(dto);

            try
            {
                var config = await _gitHubConfigRepository.GetByProjectId(dto.ProjectId)
                    ?? throw new InvalidOperationException($"GitHub configuration missing for ProjectId: {dto.ProjectId}");

                var review = await _gitHubService.SubmitReviewAsync(dto, config);

                if (review == null)
                    return null;

                var savedReview = await _pullRequestRepository.SaveReviewAsync(review);
                if (savedReview != null)
                {
                    var updatedPr = await _pullRequestRepository.UpdatePrStatus(review.PRId, review.Decision);

                    if (updatedPr == null)
                    {
                        throw new InvalidOperationException($"Failed to update PR status for PRId: {review.PRId}");
                    }
                }

                return review;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting review for PR ID: {PrId}", dto.PrID);
                throw;
            }
        }
    }
}