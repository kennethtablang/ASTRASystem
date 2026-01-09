using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Common
{
    public class CreateWarehouseDto
    {
        [Required]
        public long DistributorId { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
