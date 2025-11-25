namespace ASTRASystem.DTO.Payment
{
    public class AccountsReceivableSummaryDto
    {
        public decimal TotalOutstanding { get; set; }
        public decimal Current { get; set; } // 0-30 days
        public decimal Aging30 { get; set; } // 31-60 days
        public decimal Aging60 { get; set; } // 61-90 days
        public decimal Aging90Plus { get; set; } // >90 days
        public int TotalInvoices { get; set; }
        public int OverdueInvoices { get; set; }
    }
}
