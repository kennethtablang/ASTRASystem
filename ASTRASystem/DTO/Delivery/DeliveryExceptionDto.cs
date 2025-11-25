namespace ASTRASystem.DTO.Delivery
{
    public class DeliveryExceptionDto
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string ExceptionType { get; set; } // "StoreClosed", "Refused", "PartialDelivery", "Damaged"
        public string Description { get; set; }
        public string? ReportedById { get; set; }
        public string? ReportedByName { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
