using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class InventoryMovement : BaseEntity
    {
        [Required]
        public long InventoryId { get; set; }
        [ForeignKey(nameof(InventoryId))]
        public Inventory Inventory { get; set; }

        [Required]
        [MaxLength(100)]
        public string MovementType { get; set; } // "Restock", "Adjustment", "Order", "Transfer", "Return", "Damage"

        public int Quantity { get; set; }

        public int PreviousStock { get; set; }

        public int NewStock { get; set; }

        [MaxLength(500)]
        public string? Reference { get; set; } // Order ID, Transfer ID, etc.

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    }
}
