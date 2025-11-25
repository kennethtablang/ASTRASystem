using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Auth
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one digit, and one special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [MaxLength(150)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(150)]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

        public long? DistributorId { get; set; }
        public long? WarehouseId { get; set; }
    }
}
