using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class DispatchOrderDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public long TripId { get; set; }
    }
}
