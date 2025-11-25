using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Trip
{
    public class UpdateTripDto
    {
        [Required]
        public long TripId { get; set; }

        public string? DispatcherId { get; set; }

        public DateTime? DepartureAt { get; set; }

        [MaxLength(200)]
        public string? Vehicle { get; set; }

        public DateTime? EstimatedReturn { get; set; }
    }
}
