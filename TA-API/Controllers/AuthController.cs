using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAppUserService _appService;
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthController(IAppUserService appService, ILogger<AuthController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _appService = appService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDTO, CancellationToken ct)
        {
            var result = await _appService.RegisterUserAsync(userDTO, ct);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO userDTO, CancellationToken ct)
        {
            var result = await _appService.LoginUserAsync(userDTO.Email, userDTO.Password, ct);
            return Ok(result);

        }
        [HttpDelete("deleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId, CancellationToken ct)
        {
            await _appService.DeleteUserAsync(userId, ct);
            return Ok();
        }
        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO userDTO, CancellationToken ct)
        {
            //var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userDTO.Id))
            {
                return BadRequest(new ResponseDTO { Success = false, Message = "User ID not found in token." });
            }
            var result = await _appService.UpdateUserAsync(userDTO, ct);
            return Ok(result);
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _appService.GetCurrentUser(userId, ct);

            return Ok(user);
        }
        [HttpGet("githubInfo")]
        public async Task<IActionResult> GetGitHubInfo(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _appService.GetUserGitHubInfo(userId, ct);

            return Ok(user);
        }
        [HttpPost("saveGitHubInfo")]
        public async Task<IActionResult> SaveGitHubInfo(UserGitHubDTO dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            dto.UserId = userId;
            var result = await _appService.SaveUserGitHubInfo(dto, ct);
            return Ok(result);
        }
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO, CancellationToken ct)
        {
            var changePassword = await _appService.ChangePasswordAsync(changePasswordDTO, ct);
            return Ok(changePassword);
        }
        [HttpPut("changeUserRole")]
        public async Task<IActionResult> ChangeUserRole([FromBody]ChangeUserRoleDTO userRole, CancellationToken ct)
        {
            var changeRole = await _appService.ChangeUserRole(userRole.UserId, userRole.NewRole, ct);
            return Ok(changeRole);
        }
        [HttpGet("getRoles/{userId}")]
        public async Task<IActionResult> GetRoles(string userId, CancellationToken ct)
        {
            var roles = await _appService.GetAllRoles(userId, ct);
            return Ok(roles);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDTO request, CancellationToken ct)
        {
            var result = await _appService.ForgotPasswordAsync(request, ct);
            return Ok(result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDTO request, CancellationToken ct)
        {
           var result = await _appService.ResetPasswordAsync(request, ct);
            return Ok(result);
        }
    }
}
