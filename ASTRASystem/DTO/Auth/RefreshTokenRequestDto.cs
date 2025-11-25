using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
