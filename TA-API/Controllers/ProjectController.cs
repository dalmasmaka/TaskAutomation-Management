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
        public async Task<IActionResult> GetAllProjects()
        {
            var companyId = int.Parse(User.FindFirstValue("companyId"));

            //if (string.IsNullOrEmpty(companyId))
            //    return Unauthorized();
            var result = await _projectService.GetAllAsync(companyId);
            return Ok(result);
        }
        [HttpGet("availableProjects/{userId}")]
        public async Task<IActionResult> GetAvailableProjectsPerUser(string userId)
        {
            var companyId = int.Parse(User.FindFirstValue("companyId"));

            //if (string.IsNullOrEmpty(companyId))
            //    return Unauthorized();
            var result = await _projectService.GetProjectsAvailableToAssignToUser(companyId, userId);
            return Ok(result);
        }
        [HttpGet("availableUsers/{projectId}")]
        public async Task<IActionResult> GetAvailableUsersPerProject(int projectId)
        {
            var companyId = int.Parse(User.FindFirstValue("companyId"));
            var result = await _projectService.GetUsersAvailableToAssignToProjects(companyId, projectId);
            return Ok(result);
        }
        [HttpGet("membersPerProject/{projectId}")]
        public async Task<IActionResult> GetAllMembers(int projectId)
        {
            var result = await _projectService.GetMembersAsync(projectId);
            return Ok(result);
        }
        [HttpPost("assignUserToProjects")]
        public async Task<IActionResult> AssignUserToProjects([FromBody] AssignUserProjectsDTO dto)
        {
            try
            {
               await _projectService.AssignUserToProjects(dto);

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
        public async Task<IActionResult> AssignUsersToProject([FromBody] AssignProjectUsersDTO dto)
        {
            try
            {
                await _projectService.AssignUsersToProject(dto);

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
        [HttpDelete("removeMember/{projectId}/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId, int projectId) {
            await _projectService.RemoveMemberAsync(userId, projectId);
            return Ok();
        }
    }
}
