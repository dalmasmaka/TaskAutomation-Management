using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;

namespace TA_API.Controllers
{
    //[ApiController]
    [Route("api/webhooks")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class WebhooksController : ControllerBase
    {
        private readonly ITasksService _taskService;
        private readonly IProjectService _projectService;
        public WebhooksController(ITasksService taskService, IProjectService projectService)
        {
            _taskService = taskService;
            _projectService = projectService;
        }
        [HttpGet("trello/{projectId}")]
        [HttpHead("trello/{projectId}")]
        public IActionResult Validate(int projectId)
        {
            return Ok();
        }
        [HttpPost("trello/{projectId}")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Receive(int projectId, [FromBody] TrelloWebhookPayload payload)
        {
            // Trello triggers different types (createCard, updateCard, etc.)
            // We pass the whole payload to let the service decide the logic
            if (payload?.Action?.Data?.Card != null)
            {
                //kjo e kthen project ID direkt prej webhook ndersa ne jira funksionon per app dmth smundet me dite project id vec e qon idn e board te vet te na e rujm ExternalBoardId
                await _taskService.ProcessTrelloWebhookAsync(projectId, payload);
                return Ok();
            }

            return Ok();
        }
        [HttpPost("jira/receive")]
        public async Task<IActionResult> Receive([FromBody] System.Text.Json.JsonElement rawJson)
        {
            if (rawJson.ValueKind == JsonValueKind.Undefined ||
          rawJson.ValueKind == JsonValueKind.Null)
            {
                return BadRequest("Empty payload");
            }
            else
            {

                var jsonString = rawJson.GetRawText();

                // Manually deserialize to see the error
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var payload = JsonSerializer.Deserialize<JiraWebhookPayload>(jsonString, options);
                if (payload != null)
                {

                    var jiraProjectId = payload.Issue?.Fields?.Project?.Id;
                    if (string.IsNullOrEmpty(jiraProjectId)) return Ok();

                    var localProject = await _projectService.GetProjectByExternalId(jiraProjectId);
                    if (localProject != null)
                    {
                        var result = await _taskService.SaveJiraWebhookTask(localProject.ProjectId, payload);
                    }

                }
                return Ok();
            }
        }

        //[AllowAnonymous]
        //[HttpPost("jira/receive")]
        //public IActionResult Receive() // Remove all parameters
        //{
        //    // Put a breakpoint here
        //    return Ok();
        //}
    }
}
