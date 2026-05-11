using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITasksService _tasksService;
        private readonly ICodeGenerationService _codeGenerationService;
        public TaskController(ITasksService tasksService, ICodeGenerationService codeGenerationService)
        {
            _tasksService = tasksService;
            _codeGenerationService = codeGenerationService;
        }
        [Authorize]
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasksAsync([FromQuery] TaskQuery query)
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

            var result = await _tasksService.GetTasksAsync(query, userId, role, companyId);

            return Ok(result);
        }
        [HttpGet("companyAssets")]
        public async Task<ActionResult> GetUsersAndProjectsAsync()
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var role = User.FindFirstValue(ClaimTypes.Role);
            var companyId = int.Parse(companyIdClaim);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId claim missing");

            if (string.IsNullOrWhiteSpace(role))
                return Unauthorized("Role claim missing");

            var result = await _tasksService.GetUsersAndProjectsAsync(companyId, role, userId);
            return Ok(result);

        }

        [HttpDelete("deleteTask/{taskId}")]
        public async Task<IActionResult> DeleteAsync(int taskId)
        {
            try
            {
                await _tasksService.DeleteAsync(taskId);
                return Ok(new
                {
                    Message = "Task deleted successfully!"
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
        [HttpPut("updateTask")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateTaskDTO dto)
        {
            var result = await _tasksService.UpdateAsync(dto);
            return Ok(result);
        }
        [HttpGet("projectTasks/{projectId}")]
        public async Task<IActionResult> GetProjectTasks(int projectId)
        {
            return Ok();
        }
        [HttpGet("assignees/{taskId}")]
        public async Task<IActionResult> GetAllMembersAsync(int taskId)
        {
            var result = await _tasksService.GetAllMembersAsync(taskId);
            return Ok(result);
        }
        [HttpDelete("removeMember/{userId}/{taskId}")]
        public async Task<IActionResult> RemoveMemberAsync(string userId, int taskId)
        {
            try
            {
                await _tasksService.RemoveMember(userId, taskId);
                return Ok(new
                {
                    Message = "Member removed successfully!"
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

        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateCodeAsync([FromBody] CodeGenerationRequestDTO request)
        {
            try
            {
                var result = await _codeGenerationService.GenerateAsync(request.Description);
                return Ok(result);
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
    }
}
