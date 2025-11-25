namespace ASTRASystem.DTO.Payment
{
    public class InvoiceDto
    {
        public long Id { get; set; }
        public long? OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? InvoiceUrl { get; set; }
        public string? OrderReference { get; set; }
        public string? StoreName { get; set; }
    }
}
