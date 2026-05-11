using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TA_Domain.Entities;

namespace TA_Infrastructure.Repositories
{
    public class PullRequestRepository : IPullRequestRepository
    {
        private readonly ApplicationDbContext _context;
        public PullRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<PullRequestListDTO> GetAllPRsAsync(string userId, string role, int companyId)
        {
            var prs = _context.PullRequests
                .Where(p => !p.IsDeleted)
                .OrderByDescending(i => i.Id)
                .AsQueryable();

            if (role == "Owner")
            {
                prs = prs.Where(x => x.Task.Project.CompanyId == companyId);
            } 
            else if (role == "Developer" || role == "developer")
            {
                prs = prs.Where(x => x.CreatedById == userId &&
                    x.Task.TaskMembers.Any(m => m.UserId == userId && !m.IsDeleted)
                );
            }

            var dto = prs.Select(x => new PullRequestListDTO
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
            return dto;
        }

        public async Task<PullRequestDTO> GetPullRequestById(int id)
        {
            return await _context.PullRequests
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new PullRequestDTO
                {
                    Id = p.ExternalId,
                    TaskId = p.TaskId,
                    BranchName = p.BranchName,
                    PRUrl = p.PRUrl,
                    PRName = p.PRName,
                    PRDescription = p.PRDescription,
                    Status = p.Status,
                    ExternalId = p.ExternalId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<ReviewDTO>> GetReviewsForPullRequestAsync(int pullRequestId)
        {
            return await _context.Reviews
                .Where(r => r.PullRequestId == pullRequestId && !r.IsDeleted)
                .Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    PRId = r.PullRequestId,
                    Comment = r.Comment,
                    Decision = r.Decision,
                    Reviewer = r.CreatedBy.FullName
                })
                .ToListAsync();
        }

        public async Task<PullRequestDTO> SavePullRequestAsync(PullRequestDTO request)
        {
            var entity = new TA_Domain.Entities.PullRequest
            {
                ExternalId = request.Id,
                TaskId = request.TaskId,
                BranchName = request.BranchName,
                PRUrl = request.PRUrl,
                PRName = request.PRName,
                PRDescription = request.PRDescription,
                Status = request.Status
            };

            _context.PullRequests.Add(entity);
            await _context.SaveChangesAsync();

            return new PullRequestDTO
            {
                Id = entity.ExternalId,
                TaskId = entity.TaskId,
                BranchName = entity.BranchName,
                PRUrl = entity.PRUrl,
                PRName = entity.PRName,
                PRDescription = entity.PRDescription,
                Status = entity.Status
            };
        }

        public async Task<ReviewDTO> SaveReviewAsync(ReviewDTO review)
        {
            var existing = await _context.Reviews
               .FirstOrDefaultAsync(pi => pi.PullRequestId == review.PRId);

            if (existing != null)
            {
                // Update existing record
                existing.Comment = review.Comment;
                existing.Decision = review.Decision;
                existing.UpdatedAt = DateTime.Now;
                existing.UpdatedById = review.UserId;

                await _context.SaveChangesAsync();

                review.ReviewId = existing.ReviewId;
                return review;
            }
            else
            {
                // Create new record
                var newReview = new Review
                {
                    PullRequestId = review.PRId,
                    Comment = review.Comment,
                    Decision = review.Decision,
                    CreatedById = review.UserId,
                    CreatedAt = DateTime.Now,
                   
                };

                await _context.Reviews.AddAsync(newReview);
                await _context.SaveChangesAsync();

                review.ReviewId = newReview.ReviewId;
                return review;
            }
        }
         
        
    }
}
