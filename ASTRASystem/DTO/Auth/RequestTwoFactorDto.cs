using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Auth
{
    public class RequestTwoFactorDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
