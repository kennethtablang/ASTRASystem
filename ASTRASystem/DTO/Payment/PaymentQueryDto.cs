using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Payment
{
    public class PaymentQueryDto
    {
        public long? OrderId { get; set; }
        public long? StoreId { get; set; }
        public PaymentMethod? Method { get; set; }
        public DateTime? RecordedFrom { get; set; }
        public DateTime? RecordedTo { get; set; }
        public bool? IsReconciled { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "RecordedAt";
        public bool SortDescending { get; set; } = true;
    }
}
