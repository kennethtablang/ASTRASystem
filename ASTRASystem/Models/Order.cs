using ASTRASystem.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Order : BaseEntity
    {
        [Required]
        public long StoreId { get; set; }
        [ForeignKey(nameof(StoreId))]
        public Store Store { get; set; }

        [MaxLength(450)]
        public string AgentId { get; set; }

        public long? DistributorId { get; set; }
        [ForeignKey(nameof(DistributorId))]
        public Distributor Distributor { get; set; }

        public long? WarehouseId { get; set; }
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse Warehouse { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public bool Priority { get; set; } = false;

        public DateTime? ScheduledFor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // NEW: Payment Status Fields
        /// <summary>
        /// Indicates if the order has been fully paid
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Date when the order was fully paid
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// User who marked the order as paid
        /// </summary>
        [MaxLength(450)]
        public string? PaidById { get; set; }

        /// <summary>
        /// Total amount paid (calculated from Payments collection)
        /// </summary>
        [NotMapped]
        public decimal TotalPaid => Payments?.Sum(p => p.Amount) ?? 0;

        /// <summary>
        /// Remaining balance to be paid
        /// </summary>
        [NotMapped]
        public decimal RemainingBalance => Total - TotalPaid;

        /// <summary>
        /// Whether the order has partial payment
        /// </summary>
        [NotMapped]
        public bool HasPartialPayment => TotalPaid > 0 && TotalPaid < Total;

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<DeliveryPhoto> DeliveryPhotos { get; set; } = new List<DeliveryPhoto>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
