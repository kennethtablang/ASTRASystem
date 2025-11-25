using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.User
{
    public class UpdateUserProfileDto
    {
        [Required]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [MaxLength(150)]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(150)]
        public string LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public long? DistributorId { get; set; }
        public long? WarehouseId { get; set; }
    }
}
