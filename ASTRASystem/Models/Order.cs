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

        /// <summary>
        /// Agent who created the order (ApplicationUser.Id as string if you use string keys).
        /// If you use numeric keys for Identity, change type accordingly.
        /// </summary>
        [MaxLength(450)]
        public string AgentId { get; set; }

        /// <summary>
        /// Distributor responsible for fulfilling this order.
        /// </summary>
        public long? DistributorId { get; set; }
        [ForeignKey(nameof(DistributorId))]
        public Distributor Distributor { get; set; }

        /// <summary>
        /// Warehouse assigned for fulfilment (may be null until assigned).
        /// </summary>
        public long? WarehouseId { get; set; }
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse Warehouse { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public bool Priority { get; set; } = false;

        public DateTime? ScheduledFor { get; set; }

        // Totals (cached for reporting)
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<DeliveryPhoto> DeliveryPhotos { get; set; } = new List<DeliveryPhoto>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
