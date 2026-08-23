using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiEvent.Application.DTOs.Auth;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Enums;

namespace WebApiEvent.Presentation.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(IAuthService _authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            await _authService.RegisterAsync(request, UserRole.User, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            await _authService.RegisterAsync(request, UserRole.Admin, cancellationToken);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
