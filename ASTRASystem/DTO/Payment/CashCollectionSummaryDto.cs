namespace ASTRASystem.DTO.Payment
{
    public class CashCollectionSummaryDto
    {
        public long? TripId { get; set; }
        public string? DispatcherId { get; set; }
        public string? DispatcherName { get; set; }
        public DateTime CollectionDate { get; set; }
        public decimal TotalCash { get; set; }
        public decimal TotalGCash { get; set; }
        public decimal TotalMaya { get; set; }
        public decimal TotalBankTransfer { get; set; }
        public decimal TotalOther { get; set; }
        public decimal GrandTotal { get; set; }
        public int PaymentCount { get; set; }
    }
}
