using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Inventory : BaseEntity
    {
        [Required]
        public long ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [Required]
        public long WarehouseId { get; set; }
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse Warehouse { get; set; }
        public int StockLevel { get; set; } = 0;

        public int ReorderLevel { get; set; } = 0;

        public int MaxStock { get; set; } = 1000;

        public DateTime? LastRestocked { get; set; }

        public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
    }
}
