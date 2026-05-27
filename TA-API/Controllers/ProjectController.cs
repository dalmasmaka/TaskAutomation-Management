using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;
using TA_Application.Services;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }
        [HttpGet("projects")]
        public async Task<IActionResult> GetAllProjects(CancellationToken ct)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _projectService.GetAllAsync(companyId, userId, ct);
            return Ok(result);
        }
        [HttpGet("userProjects")]
        public async Task<IActionResult> GetUserProjectsAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var userProjects = _projectService.GetUserProjectsAsync(companyId, userId, ct);
            return Ok(userProjects);
        }

        [HttpGet("availableProjects/{userId}")]
        public async Task<IActionResult> GetAvailableProjectsPerUser(string userId, CancellationToken ct)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var result = await _projectService.GetProjectsAvailableToAssignToUser(companyId, userId, ct);
            return Ok(result);
        }
        [HttpGet("availableUsers/{projectId}")]
        public async Task<IActionResult> GetAvailableUsersPerProject(int projectId, CancellationToken ct)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var result = await _projectService.GetUsersAvailableToAssignToProject(companyId, projectId, ct);
            return Ok(result);
        }
        [HttpGet("membersPerProject/{projectId}")]
        public async Task<IActionResult> GetAllMembers(int projectId, CancellationToken ct)
        {
            var result = await _projectService.GetMembersAsync(projectId, ct);
            return Ok(result);
        }
        [HttpPost("assignUserToProjects")]
        public async Task<IActionResult> AssignUserToProjects([FromBody] AssignUserProjectsDTO dto, CancellationToken ct)
        {
            try
            {
               await _projectService.AssignUserToProjects(dto, ct);

                return Ok(new
                {
                    message = "User successfully assigned to projects"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Something went wrong"
                });
            }
        }
        [HttpPost("assignUsersToProject")]
        public async Task<IActionResult> AssignUsersToProject([FromBody] AssignProjectUsersDTO dto, CancellationToken ct)
        {
            var result = await _projectService.AssignUsersToProject(dto, ct);
            return Ok(result);
        }
        [HttpDelete("removeMember/{projectId}/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId, int projectId, CancellationToken ct) {
            await _projectService.RemoveMemberAsync(userId, projectId, ct);
            return Ok();
        }
        [HttpPost("saveProject")]
        public async Task<IActionResult> SaveProject(CreateProjectDTO dto, CancellationToken ct)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _projectService.SaveProjectAsync(userId, companyId, dto, ct);
            return Ok(result);
        }
    }
}
