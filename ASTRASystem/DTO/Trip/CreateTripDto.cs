using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Trip
{
    public class CreateTripDto
    {
        [Required]
        public long WarehouseId { get; set; }

        [Required]
        public string DispatcherId { get; set; }

        public DateTime? DepartureAt { get; set; }

        [MaxLength(200)]
        public string? Vehicle { get; set; }

        public DateTime? EstimatedReturn { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Trip must contain at least one order")]
        public List<long> OrderIds { get; set; } = new();
    }
}
