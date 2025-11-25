using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Payment
{
    public class PaymentDto
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string? Reference { get; set; }
        public string? RecordedById { get; set; }
        public string? RecordedByName { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
