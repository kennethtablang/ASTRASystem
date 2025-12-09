using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Location
{
    public class CreateBarangayDto
    {
        [Required(ErrorMessage = "Barangay name is required")]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "City is required")]
        public long CityId { get; set; }

        [MaxLength(100)]
        public string? ZipCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
