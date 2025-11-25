using ASTRASystem.DTO.Auth;
using ASTRASystem.DTO.Common;
using ASTRASystem.Models;

namespace ASTRASystem.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<ApiResponse<bool>> RequestTwoFactorCodeAsync(RequestTwoFactorDto request);
        Task<ApiResponse<AuthResponseDto>> VerifyTwoFactorAsync(VerifyTwoFactorDto request);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto request);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request);
        Task<ApiResponse<bool>> ConfirmEmailAsync(ConfirmEmailDto request);
        Task<ApiResponse<bool>> ResendConfirmationEmailAsync(string email);
        Task<ApiResponse<bool>> LogoutAsync(string userId);
    }
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
        Task<string?> ValidateRefreshTokenAsync(string refreshToken);
        Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt);
        Task RevokeRefreshTokenAsync(string userId);
        string? GetUserIdFromToken(string token);
    }

    
}
