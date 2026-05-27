using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardInsightsController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardInsightsController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        
        [HttpGet("insights")]
        public async Task<IActionResult> GetInsightsAsync([FromQuery] int? teamId, CancellationToken ct) {
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
            var result = await _dashboardService.GetDashboardInsightsAsync(userId, role, companyId, teamId, ct);
            return Ok(result);
        }
        [HttpGet("workload")]
        public async Task<IActionResult> GetUsersWorkload([FromQuery] TimeRange range, [FromQuery] int? teamId, CancellationToken ct)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrWhiteSpace(companyIdClaim))
                return Unauthorized("CompanyId claim missing");


            if (!int.TryParse(companyIdClaim, out var companyId))
                return BadRequest("Invalid companyId claim");
            var result = await _dashboardService.GetWorkloadInsightsAsync(range, companyId, teamId, ct);
            return Ok(result);
        }
        [HttpGet("prs")]
        public async Task<IActionResult> GetUsersReviews([FromQuery] TimeRange range, [FromQuery] int? teamId, CancellationToken ct)
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
            var result = await _dashboardService.GetPREngagementAsync(companyId, userId, role, range, teamId, ct);
            return Ok(result);
        }
    }
}
