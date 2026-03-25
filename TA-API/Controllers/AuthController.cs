using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TA_Application.DTOs;
using TA_Application.Services;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationUserService _appService;
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthController(ApplicationUserService appService, ILogger<AuthController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _appService = appService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDTO)
        {
            var result = await _appService.RegisterUserAsync(userDTO);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO userDTO)
        {
            var result = await _appService.LoginUserAsync(userDTO.UserName, userDTO.Password);
            return Ok(result);

        }
        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new AuthResponseDTO { Success = false, Message = "User ID not found in token." });
            }
            await _appService.DeleteUserAsync(userId);
            return Ok(new AuthResponseDTO { Success = true, Message = "User deleted successfully." });
        }
        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDTO userDTO)
        {
            //var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userDTO.Id))
            {
                return BadRequest(new AuthResponseDTO { Success = false, Message = "User ID not found in token." });
            }
            var result = await _appService.UpdateUserAsync(userDTO);
            return Ok(result);
        }
    }
}
