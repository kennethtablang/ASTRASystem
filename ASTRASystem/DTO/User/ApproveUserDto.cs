using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.User
{
    public class ApproveUserDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public bool Approve { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }
    }
}
