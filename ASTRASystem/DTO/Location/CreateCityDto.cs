using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Location
{
    public class CreateCityDto
    {
        [Required(ErrorMessage = "City name is required")]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string? Province { get; set; }

        [MaxLength(200)]
        public string? Region { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
