using Microsoft.AspNetCore.Mvc;
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
    }
}
