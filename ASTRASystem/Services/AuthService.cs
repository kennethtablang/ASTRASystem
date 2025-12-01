using ASTRASystem.DTO.Auth;
using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace ASTRASystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            IAuditLogService auditLogService,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _auditLogService = auditLogService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Email is already registered",
                        new List<string> { "A user with this email already exists" });
                }

                // Map DTO to ApplicationUser
                var user = _mapper.Map<ApplicationUser>(request);
                user.UserName = request.Email;
                user.IsApproved = false; // Require approval

                // Create user
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Registration failed",
                        result.Errors.Select(e => e.Description).ToList());
                }

                // Assign role
                await _userManager.AddToRoleAsync(user, request.Role);

                // Generate email confirmation token
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // In production, send email with confirmation link
                // await _emailService.SendConfirmationEmailAsync(user, confirmationLink);

                await _auditLogService.LogActionAsync(user.Id, "User registered", new { Email = user.Email, Role = request.Role });

                return ApiResponse<AuthResponseDto>.SuccessResponse(
                    null,
                    "Registration successful. Please wait for admin approval and confirm your email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", request.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred during registration");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid credentials");
                }

                // Check if user is approved
                if (!user.IsApproved)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse(
                        "Account pending approval",
                        new List<string> { user.ApprovalMessage ?? "Your account is awaiting administrator approval" });
                }

                // Verify password
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return ApiResponse<AuthResponseDto>.ErrorResponse("Account locked due to multiple failed attempts");
                    }
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid credentials");
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(60);

                // Store refresh token
                await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, expiresAt.AddDays(7));

                var response = _mapper.Map<AuthResponseDto>(user);
                response.Role = role;
                response.AccessToken = accessToken;
                response.RefreshToken = refreshToken;
                response.ExpiresAt = expiresAt;

                await _auditLogService.LogActionAsync(user.Id, "User logged in", new { Email = user.Email });

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred during login");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            try
            {
                var userId = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
                if (userId == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid refresh token");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.IsApproved)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("User not found or not approved");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";

                var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(60);

                await _tokenService.StoreRefreshTokenAsync(user.Id, newRefreshToken, expiresAt.AddDays(7));

                var response = _mapper.Map<AuthResponseDto>(user);
                response.Role = role;
                response.AccessToken = newAccessToken;
                response.RefreshToken = newRefreshToken;
                response.ExpiresAt = expiresAt;

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred while refreshing token");
            }
        }

        public async Task<ApiResponse<bool>> RequestTwoFactorCodeAsync(RequestTwoFactorDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal if user exists
                    return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a code has been sent");
                }

                // Generate 6-digit code
                var code = GenerateSixDigitCode();
                var codeHash = HashCode(code);

                user.TwoFactorCodeHash = codeHash;
                user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(10);
                user.TwoFactorAttempts = 0;

                await _userManager.UpdateAsync(user);
                await _emailService.SendTwoFactorCodeAsync(user, code);

                return ApiResponse<bool>.SuccessResponse(true, "Two-factor code sent to your email");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting two-factor code");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> VerifyTwoFactorAsync(VerifyTwoFactorDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid request");
                }

                if (user.TwoFactorCodeExpiry == null || user.TwoFactorCodeExpiry < DateTime.UtcNow)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Code has expired");
                }

                if (user.TwoFactorAttempts >= 3)
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Too many failed attempts. Request a new code");
                }

                var codeHash = HashCode(request.Code);
                if (user.TwoFactorCodeHash != codeHash)
                {
                    user.TwoFactorAttempts++;
                    await _userManager.UpdateAsync(user);
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid code");
                }

                // Clear 2FA data
                user.TwoFactorCodeHash = null;
                user.TwoFactorCodeExpiry = null;
                user.TwoFactorAttempts = 0;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";

                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(60);

                await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, expiresAt.AddDays(7));

                var response = _mapper.Map<AuthResponseDto>(user);
                response.Role = role;
                response.AccessToken = accessToken;
                response.RefreshToken = refreshToken;
                response.ExpiresAt = expiresAt;

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Verification successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying two-factor code");
                return ApiResponse<AuthResponseDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal if user exists
                    return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a reset link has been sent");
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                // In production, send email with reset link
                // await _emailService.SendPasswordResetEmailAsync(user, resetLink);

                await _auditLogService.LogActionAsync(user.Id, "Password reset requested", null);

                return ApiResponse<bool>.SuccessResponse(true, "Password reset email sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Invalid request");
                }

                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Password reset failed",
                        result.Errors.Select(e => e.Description).ToList());
                }

                await _auditLogService.LogActionAsync(user.Id, "Password reset completed", null);

                return ApiResponse<bool>.SuccessResponse(true, "Password reset successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Password change failed",
                        result.Errors.Select(e => e.Description).ToList());
                }

                await _auditLogService.LogActionAsync(user.Id, "Password changed", null);

                return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ConfirmEmailAsync(ConfirmEmailDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                var result = await _userManager.ConfirmEmailAsync(user, request.Token);
                if (!result.Succeeded)
                {
                    return ApiResponse<bool>.ErrorResponse("Email confirmation failed");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Email confirmed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ResendConfirmationEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a confirmation link has been sent");
                }

                if (user.EmailConfirmed)
                {
                    return ApiResponse<bool>.ErrorResponse("Email is already confirmed");
                }

                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // In production, send email with confirmation link
                // await _emailService.SendConfirmationEmailAsync(user, confirmationLink);

                return ApiResponse<bool>.SuccessResponse(true, "Confirmation email sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending confirmation email");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string userId)
        {
            try
            {
                await _tokenService.RevokeRefreshTokenAsync(userId);
                await _auditLogService.LogActionAsync(userId, "User logged out", null);
                return ApiResponse<bool>.SuccessResponse(true, "Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        private string GenerateSixDigitCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        private string HashCode(string code)
        {
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(code);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
