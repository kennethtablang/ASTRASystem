using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Delivery
{
    public class LocationUpdateDto
    {
        [Required]
        public long TripId { get; set; }

        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public double? Speed { get; set; }
        public double? Accuracy { get; set; }
    }
}
