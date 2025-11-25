using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Order
{
    public class OrderDto
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string? StoreBarangay { get; set; }
        public string? StoreCity { get; set; }
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }
        public long? DistributorId { get; set; }
        public string? DistributorName { get; set; }
        public long? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public OrderStatus Status { get; set; }
        public bool Priority { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
