using CelebrateHub.Services.DTOs;
using CelebrateHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CelebrateHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService auth, ILogger<AccountController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Validation failed."));

            var result = await _auth.RegisterAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(result.Message));

            _logger.LogInformation("New user registered: {Email}", dto.Email);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, result.Message));
        }

        /// <summary>Authenticate and receive a JWT token.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Validation failed."));

            var result = await _auth.LoginAsync(dto);
            if (!result.Success)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.Message));

            return Ok(ApiResponse<AuthResponseDto>.Ok(result, result.Message));
        }

        /// <summary>Change the current user's password. Requires authentication.</summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Validation failed."));

            // Extract the caller's ID from the JWT claims
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int employeeId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

            var success = await _auth.ChangePasswordAsync(employeeId, dto);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail("Current password is incorrect."));

            return Ok(ApiResponse<object>.Ok(new { }, "Password changed successfully."));
        }
    }
}