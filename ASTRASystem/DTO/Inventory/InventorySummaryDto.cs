namespace ASTRASystem.DTO.Inventory
{
    public class InventorySummaryDto
    {
        public int TotalProducts { get; set; }
        public int InStock { get; set; }
        public int LowStock { get; set; }
        public int OutOfStock { get; set; }
        public int Overstocked { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }
}
