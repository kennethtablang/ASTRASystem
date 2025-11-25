namespace ASTRASystem.DTO.Payment
{
    public class ARAgingLineDto
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string? StoreBarangay { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal Current { get; set; }
        public decimal Aging30 { get; set; }
        public decimal Aging60 { get; set; }
        public decimal Aging90Plus { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal AvailableCredit => CreditLimit - TotalOutstanding;
        public int InvoiceCount { get; set; }
    }
}
