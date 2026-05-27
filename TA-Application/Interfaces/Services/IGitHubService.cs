using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Services
{
    public interface IGitHubService
    {
        Task<PullRequest> CreatePullRequestAsync(PullRequestCreationDTO pr, GitHubConfig config, UserGitHubConfig userConfig);
        Task<Review> SubmitReviewAsync(Review review, GitHubConfig config, CancellationToken ct);
    }
}
