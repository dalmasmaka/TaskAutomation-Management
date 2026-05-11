using Octokit;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TA_Application.Services
{
    public class PullRequestService : IPullRequestService
    {
        private readonly IGitHubService _gitHubService;
        private readonly IGitHubConfigRepository _gitHubConfigRepository;
        private readonly IPullRequestRepository _pullRequestRepository;

        public PullRequestService(
            IGitHubService gitHubService,
            IGitHubConfigRepository gitHubConfigRepository,
            IPullRequestRepository pullRequestRepository)
        {
            _gitHubService = gitHubService;
            _gitHubConfigRepository = gitHubConfigRepository;
            _pullRequestRepository = pullRequestRepository;
        }

        public async Task<PullRequestDTO> CreatePullRequestAsync(PullRequestCreationDTO dto)
        {
            if(dto == null)
                throw new ArgumentNullException(nameof(dto), "Pull request creation data cannot be null.");
            try
            {
                var config = await _gitHubConfigRepository.GetByProjectId(dto.ProjectId);

                if (config == null)
                    throw new Exception("GitHub is not configured for this project.");

                var pr = await _gitHubService.CreatePullRequestAsync(dto, config);

                if (pr == null) return null;

                await _pullRequestRepository.SavePullRequestAsync(pr);

                return pr;
            }
            catch (Exception ex) {
                throw ex;
            }
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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Pull request creation data cannot be null.");
            try
            {
                var config = await _gitHubConfigRepository.GetByProjectId(dto.ProjectId);

                if (config == null)
                    throw new Exception("GitHub is not configured for this project.");

                var review  = await _gitHubService.SubmitReviewAsync(dto, config);

                if (review == null) return null;

                await _pullRequestRepository.SaveReviewAsync(review);
                return review;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw ex;
            }
        }
    }
}