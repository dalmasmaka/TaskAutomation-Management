using Microsoft.AspNetCore.Mvc;
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
            return Ok(new { url = await service.GetAuthUrlAsync() });
        }

        // STEP B: The Callback (Handle the return from Jira/Trello)
        [HttpGet("callback/{provider}")]
        public async Task<IActionResult> Callback(string provider, [FromQuery] string code, [FromQuery] int state)
        {
            // 'state' is the projectId we passed in the Auth URL
            var companyId = int.Parse(User.FindFirst("CompanyId")!.Value);
            var service = await _factory.GetProvider(provider, companyId);

            await service.AuthorizeAndSaveAsync(code, state);

            return Redirect($"/projects/{state}/settings?success=true");
        }
    }
}