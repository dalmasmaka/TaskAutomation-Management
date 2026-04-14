using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;

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
        public async Task<IActionResult> GetCompanyByOwner() {
            var companyId = int.Parse(User.FindFirstValue("companyId"));

            //if (string.IsNullOrEmpty(companyId))
            //    return Unauthorized();
            var result = await _companyService.GetCompanyById(companyId);
            return Ok(result);
        }
        [HttpPut("updateCompany")]
        public async Task<IActionResult> UpdateBasicInformation([FromBody] CompanyDTO companyDTO)
        {
            var result = await _companyService.UpdateCompany(companyDTO);
            return Ok(result);

        }
        [HttpPut("saveCompanyConfig")]
        public async Task<IActionResult> SaveCompanyConfiguration([FromBody] CompanyConfigDTO companyDTO)
        {

            var result = await _companyService.SaveCompanyConfig(companyDTO);
            return Ok(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpGet("companyConfig/{id}")]
        public async Task<IActionResult> GetCompanyConfigurations(int id)
        {
            var result = await _companyService.GetCompanyConfig(id);
            return Ok(result);
        }
        [HttpGet("companyTeam")]
        public async Task<IActionResult> GetCompanyTeam()
        {
            var companyId = int.Parse(User.FindFirstValue("companyId"));

            //if (string.IsNullOrEmpty(companyId))
            //    return Unauthorized();
            var result = await _companyService.GetTeamOfCompany(companyId);
            return Ok(result);
        }
        [HttpGet("codes")]
        public async Task<IActionResult> GetInvitationsCode() {
            var companyId = int.Parse(User.FindFirstValue("companyId"));
            var result = await _companyService.GetInvitations(companyId);
            return Ok(result);
        }
        [HttpPost("generateCode")]
        public async Task<IActionResult> GenerateCode([FromBody] DateTime expiryDate) {

            var companyId = int.Parse(User.FindFirstValue("companyId"));
            var result = await _companyService.GenerateCodeAsync(companyId, expiryDate);
            return Ok(result);  
        }
    }
}
