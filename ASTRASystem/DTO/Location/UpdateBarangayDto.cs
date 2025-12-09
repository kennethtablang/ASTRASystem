using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Location
{
    public class UpdateBarangayDto
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public long CityId { get; set; }

        [MaxLength(100)]
        public string? ZipCode { get; set; }

        public bool IsActive { get; set; }
    }
}
