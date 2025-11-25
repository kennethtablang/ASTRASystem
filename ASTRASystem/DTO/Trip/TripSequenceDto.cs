using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Trip
{
    public class TripSequenceDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SequenceNo { get; set; }
    }
}
