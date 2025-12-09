using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Location
{
    public class BulkCreateBarangaysDto
    {
        [Required]
        public long CityId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one barangay must be provided")]
        public List<string> BarangayNames { get; set; } = new();
    }
}
