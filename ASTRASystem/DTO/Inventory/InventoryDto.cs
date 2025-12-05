namespace ASTRASystem.DTO.Inventory
{
    public class InventoryDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public string? Category { get; set; }
        public long WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int StockLevel { get; set; }
        public int ReorderLevel { get; set; }
        public int MaxStock { get; set; }
        public DateTime? LastRestocked { get; set; }
        public string Status { get; set; } // "In Stock", "Low Stock", "Out of Stock", "Overstocked"
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
