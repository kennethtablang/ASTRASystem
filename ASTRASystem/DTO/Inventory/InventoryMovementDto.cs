namespace ASTRASystem.DTO.Inventory
{
    public class InventoryMovementDto
    {
        public long Id { get; set; }
        public long InventoryId { get; set; }
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        public string MovementType { get; set; }
        public int Quantity { get; set; }
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime MovementDate { get; set; }
    }
}
