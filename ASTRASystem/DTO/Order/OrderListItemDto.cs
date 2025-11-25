using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Order
{
    public class OrderListItemDto
    {
        public long Id { get; set; }
        public string StoreName { get; set; }
        public string? StoreBarangay { get; set; }
        public string? AgentName { get; set; }
        public OrderStatus Status { get; set; }
        public bool Priority { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
