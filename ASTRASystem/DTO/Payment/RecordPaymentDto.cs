using ASTRASystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Payment
{
    public class RecordPaymentDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [MaxLength(200)]
        public string? Reference { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
