using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Order
{
    public class OrderListItemDto
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreBarangay { get; set; }
        public string StoreCity { get; set; }
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }
        public OrderStatus Status { get; set; }
        public bool Priority { get; set; }
        public int ItemCount { get; set; }
        public decimal Total { get; set; }

        // Payment Status Fields
        public bool IsPaid { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentStatus { get; set; } // "Unpaid", "Partial", "Paid"

        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
    }
}
