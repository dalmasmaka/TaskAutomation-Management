using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using TA_Application.DTOs;
using TA_Application.Services;

namespace TA_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationsController : ControllerBase
    {
        private readonly TaskProviderFactory _factory;

        public IntegrationsController(TaskProviderFactory factory) => _factory = factory;

        // STEP A: Redirect user to Jira/Trello
        [HttpGet("connect/{provider}/{projectId}/{companyId}")]
        public async Task<IActionResult> GetLocation(string provider, int projectId, int companyId)
        {
            var service = await _factory.GetProvider(provider, companyId);
            return Ok(new { url = await service.GetAuthUrlAsync(projectId, companyId) });
        }

        [HttpGet("callback/{provider}")]
        public async Task<IActionResult> Callback(string provider, string code, string state)
        {
            if (provider == "jira")
            {
                var parts = state.Split(':');

                if (parts.Length != 2)
                    return BadRequest("Invalid state");

                var projectId = int.Parse(parts[0]);
                var companyId = int.Parse(parts[1]);

                var service = await _factory.GetProvider(provider, companyId);

                await service.AuthorizeAndSaveAsync(code, projectId);

                return Content(@"
            <script>
                window.opener.postMessage('integration-success', '*');
                window.close();
            </script>", "text/html");
            }

            return BadRequest("Unsupported provider");
        }
        [HttpPost("trello/token")]
        public async Task<IActionResult> SaveTrelloToken([FromBody] TrelloTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest("Token is null");

            var service = await _factory.GetProvider("trello", request.CompanyId);

            await service.AuthorizeAndSaveAsync(request.Token, request.ProjectId);

            return Ok();
        }
        [HttpGet("boards/{provider}/{projectId}/{companyId}")]
        public async Task<IActionResult> GetBoards(string provider, int projectId, int companyId)
        {
            var service = await _factory.GetProvider(provider, companyId);

            var boards = await service.GetBoardsAsync(projectId);

            return Ok(boards);
        }
        [HttpPost("link-board")]
        public async Task<IActionResult> LinkBoard([FromBody] LinkBoardRequest request)
        {
            // 1. Get the provider service
            var service = await _factory.GetProvider(request.Provider, request.CompanyId);

            // 2. Save the board ID (This updates the Integration record in DB)
            await service.SaveBoardSelectionAsync(request.ProjectId, request.BoardId, request.ProjectKey);

            return Ok(new
            {
                message = "Board linked & tasks imported successfully",
            });
        }
        [HttpPost("sync/{projectKey}")]
        public async Task<IActionResult> SyncTasks(string projectKey, int projectId, string provider, int companyId)
        {

            return Ok();
        }
    }
}