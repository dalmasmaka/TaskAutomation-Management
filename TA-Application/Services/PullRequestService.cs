using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public async Task<PullRequestDTO> CreatePullRequestAsync(PullRequestCreationDTO dto, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var config = await _gitHubConfigRepository.GetByProjectId(dto.ProjectId, ct)
                ?? throw new InvalidOperationException("GitHub is not configured for this project.");

            var userConfig = await _gitHubConfigRepository.GetUsersGitHubConfig(dto.UserId, ct)
                ?? throw new InvalidOperationException("GitHub account is not configured for this user.");
          
            var pr = await _gitHubService.CreatePullRequestAsync(dto, config, userConfig);

            if (pr == null)
                throw new InvalidOperationException("Failed to create pull request.");


            var entity = await _pullRequestRepository.SavePullRequestAsync(pr, ct);
            var mapped = new PullRequestDTO
            {
                Id = entity.ExternalId,
                TaskId = entity.TaskId,
                BranchName = entity.BranchName,
                PRUrl = entity.PRUrl,
                PRName = entity.PRName,
                PRDescription = entity.PRDescription,
                Status = entity.Status,
                UserId = entity.CreatedById
            };
            return mapped;
        }

        public async Task<PagedResult<PullRequestListDTO>> GetAllPRAsync(PRQuery query, string userId, string role, int companyId, CancellationToken ct)
        {
            var pr = _pullRequestRepository.GetAllPRsAsync(userId, role, companyId, ct);
            var dto = pr.Select(x => new PullRequestListDTO
            {
                TaskId = x.TaskId,
                ProjectName = x.Task.Project != null ? x.Task.Project.Name : null,
                BranchName = x.BranchName,
                PrId = x.Id,
                PRName = x.PRName,
                PRDescription = x.PRDescription,
                ProjectId = x.Task.ProjectId,
                PRUrl = x.PRUrl,
                Status = x.Status,
                CreatedBy = x.CreatedBy.FullName,
                CreatedById = x.CreatedById
            });
            if (query.ProjectIds?.Any() == true)
            {
                dto = dto.Where(t => query.ProjectIds.Contains(t.ProjectId));
            }
            if (query.TaskIds?.Any() == true)
            {
                dto = dto.Where(t => query.TaskIds.Contains(t.TaskId));
            }
            if (query.CreatedByIds?.Any() == true)
            {
                dto = dto.Where(t => query.CreatedByIds.Contains(t.CreatedById));
            }
            if (!string.IsNullOrWhiteSpace(query.Search))
                dto = dto.Where(x => x.TaskId.ToString().Equals(query.Search));
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var statusList = query.Status
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());

                dto = dto.Where(t => statusList.Contains(t.Status));
            }
            var totalCount = await pr.CountAsync();

            var data = await dto
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<PullRequestListDTO>
            {
                Items = data,
                TotalCount = totalCount
            };
        }

        public async Task<List<ReviewDTO>> GetReviewsForPullRequestAsync(int pullRequestId, CancellationToken ct)
        {
            if(pullRequestId <= 0)
                throw new ArgumentException("Invalid pull request ID.");

            try
            {
                var reviews = await _pullRequestRepository.GetReviewsForPullRequestAsync(pullRequestId, ct);
                var mapped = reviews.Select(t => new ReviewDTO
                {
                    PRId = t.PullRequestId,
                    Comment = t.Comment,
                    Decision = t.Decision,
                    Developer = t.CreatedBy.FullName,
                    DeveloperId = t.CreatedById

                }).ToList();
                return mapped;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ReviewDTO> SubmitReviewAsync(ReviewRequestDTO dto, CancellationToken ct)
        {
            // 1. Guard clause: Use the built-in throw helper
            ArgumentNullException.ThrowIfNull(dto);

            try
            {
                var config = await _gitHubConfigRepository.GetByProjectId(dto.ProjectId, ct)
                    ?? throw new InvalidOperationException($"GitHub configuration missing for ProjectId: {dto.ProjectId}");
           
                var entity = new Review
                {
                    PullRequestId = dto.PrID,
                    Comment = dto.Comment,
                    Decision = dto.Decision,
                    CreatedById = dto.UserId
                };
                var review = await _gitHubService.SubmitReviewAsync(entity, config, ct);

                var savedReview = await _pullRequestRepository.SaveReviewAsync(review, ct);
                if (savedReview != null)
                {
                    var updatedPr = await _pullRequestRepository.UpdatePrStatus(review.PullRequestId, review.Decision, ct);

                    if (updatedPr == null)
                    {
                        throw new InvalidOperationException($"Failed to update PR status for PRId: {review.PullRequestId}");
                    }
                }
                var mapped = new ReviewDTO
                {
                    PRId = review.PullRequestId,
                    Comment = dto.Comment,
                    Decision = dto.Decision
                };

                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting review for PR ID: {PrId}", dto.PrID);
                throw;
            }
        }
    }
}