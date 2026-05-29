using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        public readonly ITeamService _teamService;
        public TeamController(ITeamService teamService) { 
            _teamService = teamService;
        }
        [HttpGet("teams")]
        public async Task<IActionResult> GetTeamsAsync(CancellationToken ct) {
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
            var result = await _teamService.GetAllTeams(companyId, userId, role, ct);
            return Ok(result);
        }
        [HttpGet("project-teams/{projectId}")]
        public async Task<IActionResult> GetProjectTeamsAsync(int projectId, CancellationToken ct) { 
            var result = await _teamService.GetProjectTeamsAsync(projectId, ct);
            return Ok(result);
        }
        //e pa perdorur 
        [HttpPost("add-member")]
        public async Task<IActionResult> AddMember([FromBody] AssignMemberToTeamsDTO dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");
            dto.ActionByUserId = userId;
            var result = await _teamService.AddUserToTeamsAsync(dto, ct);
            return Ok(result);
        }
        [HttpPut("update-member")]
        public async Task<IActionResult> UpdateMember([FromBody] MemberStatusUpdateDTO dto, CancellationToken ct)
        {
            var result = await _teamService.UpdateMemberStatusAsync(dto.UserId, dto.TeamId, dto.UpdateStatus, ct);
            return Ok(result);
        }
        [HttpGet("available-teams-per-project/{projectId}")]
        public async Task<IActionResult> GetAvailableTeamsForProjectAsync(int projectId, CancellationToken ct) {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (!int.TryParse(companyIdClaim, out var companyId))
                return BadRequest("Invalid companyId claim");
            var result = await _teamService.GetAvailableTeamsForProjectAsync(companyId, projectId, ct);
            return Ok(result);
        }
        [HttpDelete("unassign-team-from-project/{projectId}/{teamId}")]
        public async Task<IActionResult> UnassignProjectFromTeamAsync(int projectId, int teamId, CancellationToken ct) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");
            if (!int.TryParse(companyIdClaim, out var companyId))
                return BadRequest("Invalid companyId claim");
            var result = await _teamService.UnassignProjectFromTeamAsync(projectId, teamId, userId, ct);
            return Ok(result);
        }
        [HttpPost("assign-project-with-teams")]
        public async Task<IActionResult> AssignProjectWithTeamsAsync([FromBody] AssignProjectWithTeamsDTO dto, CancellationToken ct) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");

            var result = await _teamService.AssignProjectWithTeamsAsync(dto.ProjectId, dto.TeamIds, userId, ct);
            return Ok(result);
        }
        [HttpPost("save-team")]
        public async Task<IActionResult> SaveTeamAsync([FromBody] TeamDTO dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");
            var companyIdClaim = User.FindFirstValue("companyId");
            if (!int.TryParse(companyIdClaim, out var companyId))
                return BadRequest("Invalid companyId claim");
            dto.CompanyId = companyId;
            dto.ActionByUserId = userId;
            var result = await _teamService.SaveTeamAsync(dto, ct);
            return Ok(result);
        }
        [HttpDelete("delete-team/{teamId}")]
        public async Task<IActionResult> DeleteTeamAsync(int teamId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");
            var result = await _teamService.DeleteTeamAsync(teamId, userId, ct);
            return Ok(result);
        }
        [HttpPost("send-invitations")]
        public async Task<IActionResult> SendInvitations([FromBody] TeamInvitationsDTO dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");
            var result = await _teamService.SendTeamInvitationsAsync(dto.UserIDs, dto.TeamIDs, userId, ct);
            return Ok(result);
        }
        [HttpGet("team-members")]
        public async Task<IActionResult> GetTeamMembersAsync(CancellationToken ct)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (!int.TryParse(companyIdClaim, out var companyId))
                return BadRequest("Invalid companyId claim");

            var result = await _teamService.GetUsersOfTeams(companyId, ct);
            return Ok(result);
        }
        [HttpPut("remove-member")]
        public async Task<IActionResult> RemoveTeamMemberAsync([FromBody] RemoveTeamMemberDTO dto, CancellationToken ct)
        {
            var result = await _teamService.RemoveUserFromTeamAsync(dto.UserId, dto.TeamId, ct);
            return Ok(result);
        }
        [HttpGet("user-teams")]
        public async Task<IActionResult> GetUserTeamsAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");

            var result = await _teamService.GetUserTeams(userId, ct);
            return Ok(result);
        }
    }
}
