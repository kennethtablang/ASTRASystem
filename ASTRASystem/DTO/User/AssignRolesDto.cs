using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.User
{
    public class AssignRolesDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one role must be assigned")]
        public List<string> Roles { get; set; } = new();
    }
}
