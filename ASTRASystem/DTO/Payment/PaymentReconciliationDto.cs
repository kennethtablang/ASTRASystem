using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Payment
{
    public class PaymentReconciliationDto
    {
        public long PaymentId { get; set; }
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string? Reference { get; set; }
        public DateTime RecordedAt { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime? ReconciledAt { get; set; }
        public string? ReconciledById { get; set; }
    }
}
