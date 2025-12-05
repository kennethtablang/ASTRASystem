namespace ASTRASystem.DTO.Inventory
{
    public class InventoryQueryDto
    {
        public string? SearchTerm { get; set; }
        public long? WarehouseId { get; set; }
        public long? ProductId { get; set; }
        public string? Status { get; set; } // "All", "In Stock", "Low Stock", "Out of Stock", "Overstocked"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "ProductName";
        public bool SortDescending { get; set; } = false;
    }
}
