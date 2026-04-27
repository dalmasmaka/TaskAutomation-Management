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
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDTO)
        {
            var result = await _appService.RegisterUserAsync(userDTO);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO userDTO)
        {
            var result = await _appService.LoginUserAsync(userDTO.Email, userDTO.Password);
            return Ok(result);

        }
        [HttpDelete("deleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _appService.DeleteUserAsync(userId);
            return Ok();
        }
        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO userDTO)
        {
            //var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userDTO.Id))
            {
                return BadRequest(new ResponseDTO { Success = false, Message = "User ID not found in token." });
            }
            var result = await _appService.UpdateUserAsync(userDTO);
            return Ok(result);
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _appService.GetCurrentUser(userId);

            return Ok(user);
        }
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            var changePassword = await _appService.ChangePasswordAsync(changePasswordDTO);
            return Ok(changePassword);
        }
        [HttpPut("changeUserRole")]
        public async Task<IActionResult> ChangeUserRole([FromBody]ChangeUserRoleDTO userRole)
        {
            var changeRole = await _appService.ChangeUserRole(userRole.UserId, userRole.NewRole);
            return Ok(changeRole);
        }
        [HttpGet("getRoles/{userId}")]
        public async Task<IActionResult> GetRoles(string userId)
        {
            var roles = await _appService.GetAllRoles(userId);
            return Ok(roles);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDTO request)
        {
            var result = await _appService.ForgotPasswordAsync(request);
            return Ok(result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDTO request)
        {
           var result = await _appService.ResetPasswordAsync(request);
            return Ok(result);
        }
    }
}
