using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Order
{
    public class OrderDto
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string? StoreAddressLine1 { get; set; }
        public string? StoreAddressLine2 { get; set; }
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

        // Payment Status Fields
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaidById { get; set; }
        public string? PaidByName { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool HasPartialPayment { get; set; }
        public string PaymentStatus { get; set; } // "Unpaid", "Partial", "Paid"

        public List<OrderItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
