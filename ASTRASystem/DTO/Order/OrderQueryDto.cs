using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Order
{
    public class OrderQueryDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public OrderStatus? Status { get; set; }
        public long? StoreId { get; set; }
        public string? AgentId { get; set; }
        public long? DistributorId { get; set; }
        public long? WarehouseId { get; set; }
        public bool? Priority { get; set; }
        public DateTime? ScheduledFrom { get; set; }
        public DateTime? ScheduledTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } = "createdAt";
        public bool SortDescending { get; set; } = true;
    }
}
