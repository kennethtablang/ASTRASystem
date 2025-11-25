using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Invoice : BaseEntity
    {
        public long? OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string InvoiceUrl { get; set; }
    }
}
