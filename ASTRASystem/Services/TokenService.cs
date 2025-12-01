using ASTRASystem.Data;
using ASTRASystem.Helpers;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ASTRASystem.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ApplicationDbContext _context;

        public TokenService(IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
        }

        public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", user.FullName)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add optional claims
            if (user.DistributorId.HasValue)
            {
                claims.Add(new Claim("DistributorId", user.DistributorId.Value.ToString()));
            }

            if (user.WarehouseId.HasValue)
            {
                claims.Add(new Claim("WarehouseId", user.WarehouseId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<string?> ValidateRefreshTokenAsync(string refreshToken)
        {
            // In a production system, you'd store refresh tokens in database
            // For now, we'll use a simple cache approach
            // This is a placeholder - implement proper refresh token storage

            var hashedToken = HashToken(refreshToken);

            // Query from database (you'll need a RefreshToken entity)
            // For now, return null to indicate invalid token
            // TODO: Implement proper refresh token validation with database

            return null;
        }

        public async Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt)
        {
            // TODO: Store refresh token in database
            // You'll need to create a RefreshToken entity
            // For now, this is a placeholder

            var hashedToken = HashToken(refreshToken);

            // Create RefreshToken entity and save to database
            // await _context.RefreshTokens.AddAsync(new RefreshToken { ... });
            // await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string userId)
        {
            // TODO: Revoke all refresh tokens for user
            // Delete or mark as revoked in database
        }

        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

                return userId;
            }
            catch
            {
                return null;
            }
        }

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
