using ASTRASystem.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Payment : BaseEntity
    {
        public long OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }

        [MaxLength(200)]
        public string Reference { get; set; }

        [MaxLength(450)]
        public string RecordedById { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
