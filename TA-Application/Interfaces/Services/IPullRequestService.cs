using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IPullRequestService
    {
        Task<PullRequestDTO> CreatePullRequestAsync(PullRequestCreationDTO pullRequestCreationDTO, CancellationToken ct);
        Task<PagedResult<PullRequestListDTO>> GetAllPRAsync(PRQuery query, string userId, string role, int companyId, CancellationToken ct);
        Task<List<ReviewDTO>> GetReviewsForPullRequestAsync(int pullRequestId, CancellationToken ct);
        Task<ReviewDTO> SubmitReviewAsync(ReviewRequestDTO dto, CancellationToken ct);
    }
}
