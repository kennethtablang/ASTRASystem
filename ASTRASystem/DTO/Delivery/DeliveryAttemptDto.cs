using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Delivery
{
    public class DeliveryAttemptDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [MaxLength(200)]
        public string AttemptResult { get; set; } // "Delivered", "Failed", "Rescheduled"

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    }
}
