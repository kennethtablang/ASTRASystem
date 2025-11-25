using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Trip
{
    public class ReorderTripAssignmentsDto
    {
        [Required]
        public long TripId { get; set; }

        [Required]
        [MinLength(1)]
        public List<TripSequenceDto> Sequences { get; set; } = new();
    }
}
