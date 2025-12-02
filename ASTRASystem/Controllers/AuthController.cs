using ASTRASystem.DTO.Auth;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASTRASystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            if (!result.Success)
            {
                return Unauthorized(result);
            }
            return Ok(result);
        }

        [HttpPost("request-2fa")]
        public async Task<IActionResult> RequestTwoFactor([FromBody] RequestTwoFactorDto request)
        {
            var result = await _authService.RequestTwoFactorCodeAsync(request);
            return Ok(result);
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorDto request)
        {
            var result = await _authService.VerifyTwoFactorAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto request)
        {
            var result = await _authService.ConfirmEmailAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromQuery] string email)
        {
            var result = await _authService.ResendConfirmationEmailAsync(email);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.LogoutAsync(userId);
            return Ok(result);
        }
    }
}
