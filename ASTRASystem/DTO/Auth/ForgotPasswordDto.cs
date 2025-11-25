using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
