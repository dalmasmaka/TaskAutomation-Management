using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PullRequestController : ControllerBase
    {
        private readonly IPullRequestService _pullRequestService;
        public PullRequestController(IPullRequestService pullRequestService)
        {
            _pullRequestService = pullRequestService;
        }
        [HttpGet("pull-requests")]
        public async Task<IActionResult> GetAllPRAsync([FromQuery] PRQuery query)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(companyIdClaim))
                return Unauthorized("CompanyId claim missing");

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");

            if (string.IsNullOrWhiteSpace(role))
                return Unauthorized("Role claim missing");

            if (!int.TryParse(companyIdClaim, out var companyId))
                return BadRequest("Invalid companyId claim");

            var result = await _pullRequestService.GetAllPRAsync(query, userId, role, companyId);

            return Ok(result);
        }

        [HttpPost("create-pull-request")]
        public async Task<IActionResult> CreatePullRequest([FromBody] PullRequestCreationDTO pullRequestCreationDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");
            pullRequestCreationDTO.UserId = userId;
            // Call the service method to create a pull request
            var result = await _pullRequestService.CreatePullRequestAsync(pullRequestCreationDTO);
            if (result == null)
            {
                return BadRequest("Failed to create pull request.");
            }
            return Ok(result);
        }
        [HttpGet("reviews/{pullRequestId}")]
        public async Task<IActionResult> GetReviewsForPullRequest(int pullRequestId)
        {
            var reviews = await _pullRequestService.GetReviewsForPullRequestAsync(pullRequestId);
            if (reviews == null)
            {
                return NotFound("Pull request not found or no reviews available.");
            }
            return Ok(reviews);
        }
        [HttpPost("add-review")]
        public async Task<IActionResult> PostReview([FromBody] ReviewRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");
            dto.UserId = userId;
            var result = await _pullRequestService.SubmitReviewAsync(dto);

            //if (!result) return BadRequest("Could not submit review. PR might not exist.");

            return Ok(new { message = "Review submitted successfully" });
        }
    }
}
