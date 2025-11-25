using ASTRASystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Trip
{
    public class UpdateTripStatusDto
    {
        [Required]
        public long TripId { get; set; }

        [Required]
        public TripStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
