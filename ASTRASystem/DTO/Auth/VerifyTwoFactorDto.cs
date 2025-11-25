using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Auth
{
    public class VerifyTwoFactorDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; }
    }
}
