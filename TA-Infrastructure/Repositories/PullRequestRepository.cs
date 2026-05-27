using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TA_Infrastructure.Repositories
{
    public class PullRequestRepository : IPullRequestRepository
    {
        private readonly ApplicationDbContext _context;
        public PullRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<PullRequest> GetAllPRsAsync(string userId, string role, int companyId, CancellationToken ct)
        {
            var prs = _context.PullRequests
                .Where(p => !p.IsDeleted)
                .OrderByDescending(i => i.Id)
                .AsQueryable();


            if (role == "Developer" || role == "developer")
            {
                prs = prs.Where(x => x.CreatedById == userId &&
                    x.Task.TaskMembers.Any(m => m.UserId == userId && !m.IsDeleted)
                );
            }
            else
            {
                prs = prs.Where(x => x.Task.Project.CompanyId == companyId);
            }
            return prs;
        }
        public async Task<List<PullRequest>> GetAllPRsForInsights(List<int>taskIds, string userId, string role, CancellationToken ct)
        {
            var query = _context.PullRequests.Where(pr => !pr.IsDeleted && taskIds.Contains(pr.TaskId)).AsQueryable();

            if (role.Equals("Developer", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(pr =>
                    pr.CreatedById == userId &&
                    pr.Task != null &&
                    pr.Task.TaskMembers.Any(tm =>
                        tm.UserId == userId &&
                        !tm.IsDeleted));
            }

            return await query.ToListAsync(ct);
        }

        public async Task<PullRequest> GetPullRequestById(int id, CancellationToken ct)
        {
            return await _context.PullRequests
                .Where(p => p.Id == id && !p.IsDeleted)
                //.Select(p => new PullRequestDTO
                //{
                //    Id = p.ExternalId,
                //    TaskId = p.TaskId,
                //    BranchName = p.BranchName,
                //    PRUrl = p.PRUrl,
                //    PRName = p.PRName,
                //    PRDescription = p.PRDescription,
                //    Status = p.Status,
                //    ExternalId = p.ExternalId
                //})
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<Review>> GetReviewsForPullRequestAsync(int pullRequestId, CancellationToken ct)
        {
            return await _context.Reviews
                .Where(r => r.PullRequestId == pullRequestId && !r.IsDeleted)
                //.Select(r => new ReviewDTO
                //{
                //    ReviewId = r.ReviewId,
                //    PRId = r.PullRequestId,
                //    Comment = r.Comment,
                //    Decision = r.Decision,
                //    Reviewer = r.CreatedBy.FullName
                //})
                .ToListAsync(ct);
        }
        public async Task<List<PullRequest>> GetWorkspacePullRequests(int companyId, DateTime startDate, int? teamId, CancellationToken ct)
        {
            var query = _context.Projects
                .Where(p => p.CompanyId == companyId && !p.IsDeleted);

            if (teamId.HasValue)
            {
                query = query.Where(p =>
                    p.ProjectTeams.Any(pt => pt.TeamId == teamId.Value));
            }

            var allReviews = await query
                .SelectMany(p => p.Tasks)
                .SelectMany(t => t.PullRequests)
                .Include(r => r.CreatedBy)
                .Where(r => r.CreatedAt >= startDate)
                .ToListAsync(ct);

            return allReviews;
        }
        public async Task<PullRequest> SavePullRequestAsync(PullRequest request, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(request);

            var review = await _context.Reviews
                .Include(x => x.PullRequest)
                .FirstOrDefaultAsync(x => x.PullRequest.TaskId == request.TaskId, ct);

            // CASE 1: Review doesn't exist OR no PullRequest → CREATE
            if (review == null || review.PullRequest == null)
            {
                await _context.PullRequests.AddAsync(request, ct);
                await _context.SaveChangesAsync(ct);
                return request;
            }

            // CASE 2: PullRequest exists → UPDATE
            var pr = review.PullRequest;

            pr.ExternalId = request.ExternalId;
            pr.TaskId = request.TaskId;
            pr.BranchName = request.BranchName;
            pr.PRUrl = request.PRUrl;
            pr.PRName = request.PRName;
            pr.PRDescription = request.PRDescription;
            pr.Status = request.Status;
            pr.UpdatedById = request.CreatedById;

            await _context.SaveChangesAsync(ct);

            return pr;
        }
        public async Task<Review> SaveReviewAsync(Review review, CancellationToken ct)
        {

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> UpdatePrStatus(int pRId, string decision, CancellationToken ct)
        {

            var existing = await _context.PullRequests.FirstOrDefaultAsync(t => t.Id == pRId, ct);

            if (existing == null)
            {
                return false;
            }

            existing.Status = decision;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
