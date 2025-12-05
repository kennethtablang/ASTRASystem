using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Inventory
{
    public class AdjustInventoryDto
    {
        [Required]
        public long InventoryId { get; set; }

        [Required]
        [Range(-100000, 100000, ErrorMessage = "Quantity adjustment is out of range")]
        public int QuantityAdjustment { get; set; }

        [Required]
        [MaxLength(100)]
        public string MovementType { get; set; } // "Restock", "Adjustment", "Damage", "Return"

        [MaxLength(500)]
        public string? Reference { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
