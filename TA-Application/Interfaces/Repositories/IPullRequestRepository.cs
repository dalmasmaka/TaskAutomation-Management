using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface IPullRequestRepository
    {
        Task<PullRequest> SavePullRequestAsync(PullRequest request, CancellationToken ct);
        Task<Review> SaveReviewAsync(Review review, CancellationToken ct);
        IQueryable<PullRequest> GetAllPRsAsync(string userId, string role, int companyId, CancellationToken ct);
        Task<List<PullRequest>> GetAllPRsForInsights(List<int> taskIds, string userId, string role, CancellationToken ct);
        Task<List<Review>> GetReviewsForPullRequestAsync(int pullRequestId, CancellationToken ct);
        Task<PullRequest> GetPullRequestById(int id, CancellationToken ct);
        Task<List<PullRequest>> GetWorkspacePullRequests(int companyId, DateTime startDate, int? teamId, CancellationToken ct);
        Task<bool> UpdatePrStatus(int pRId, string decision, CancellationToken ct);
    }
}
