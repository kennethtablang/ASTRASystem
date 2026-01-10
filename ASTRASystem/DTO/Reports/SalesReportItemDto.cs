namespace ASTRASystem.DTO.Reports
{
    public class SalesReportItemDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int DeliveredOrderCount { get; set; }
        public int PendingOrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal GrowthPercentage { get; set; }
    }
}
