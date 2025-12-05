using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Inventory
{
    public class RestockInventoryDto
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        public long WarehouseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [MaxLength(500)]
        public string? Reference { get; set; } // PO number, delivery receipt, etc.

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
