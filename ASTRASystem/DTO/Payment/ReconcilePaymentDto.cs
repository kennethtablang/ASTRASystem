using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Payment
{
    public class ReconcilePaymentDto
    {
        [Required]
        public long PaymentId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
