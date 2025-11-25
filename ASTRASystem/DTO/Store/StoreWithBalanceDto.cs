namespace ASTRASystem.DTO.Store
{
    public class StoreWithBalanceDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Barangay { get; set; }
        public string? City { get; set; }
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal AvailableCredit => CreditLimit - OutstandingBalance;
        public int OverdueInvoiceCount { get; set; }
    }
}
