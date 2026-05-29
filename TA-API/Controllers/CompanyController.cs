using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;
using TA_Domain.Enums;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;

        }
        [Authorize(Roles = "Owner")]
        [HttpGet("company")]
        public async Task<IActionResult> GetCompanyByOwner(CancellationToken ct) {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var result = await _companyService.GetCompanyById(companyId, ct);
            return Ok(result);
        }
        [HttpPut("updateCompany")]
        public async Task<IActionResult> UpdateBasicInformation([FromBody] CompanyDTO companyDTO, CancellationToken ct)
        {
            var result = await _companyService.UpdateCompany(companyDTO, ct);
            return Ok(result);

        }
        [Authorize(Roles = "Owner")]
        [HttpPut("saveCompanyConfig")]
        public async Task<IActionResult> SaveCompanyConfiguration([FromBody] CompanyConfigDTO companyDTO, CancellationToken ct)
        {
            var result = await _companyService.SaveCompanyConfig(companyDTO, ct);
            return Ok(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpGet("companyConfig/{id}")]
        public async Task<IActionResult> GetCompanyConfigurations(int id, CancellationToken ct)
        {
            var result = await _companyService.GetCompanyConfig(id, ct);
            return Ok(result);
        }
        [HttpGet("companyTeam")]
        public async Task<IActionResult> GetCompanyTeam(CancellationToken ct)
        {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(role))
                return Unauthorized("Role claim missing");
            var result = await _companyService.GetTeamOfCompany(companyId, role, ct);
            return Ok(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpGet("codes")]
        public async Task<IActionResult> GetInvitationsCode(CancellationToken ct) {
            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var result = await _companyService.GetInvitations(companyId, ct);
            return Ok(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpPost("generateCode")]
        public async Task<IActionResult> GenerateCode([FromBody] GenerateInvitationDTO dto, CancellationToken ct) {

            var companyIdClaim = User.FindFirstValue("companyId");
            if (string.IsNullOrEmpty(companyIdClaim)) return Unauthorized();
            var companyId = int.Parse(companyIdClaim);
            var result = await _companyService.GenerateCodeAsync(companyId, dto.ExpiryDate, dto.Role, ct);
            return Ok(result);  
        }
        [Authorize(Roles = "Owner")]
        [HttpDelete("deleteConfig/{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct) {
            await _companyService.DeleteConfig(id, ct);
            return Ok();
        }
        [Authorize(Roles = "Owner")]
        [HttpDelete("deleteCompany/{id}")]
        public async Task<IActionResult> DeleteCompany(int id, CancellationToken ct)
        {
            await _companyService.DeleteCompanyAsync(id, ct);
            return Ok();
        }
    }
}
