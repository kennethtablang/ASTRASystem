namespace ASTRASystem.DTO.Reports
{
    public class SalesReportDto
    {
        public string ReportType { get; set; } // "Daily", "Monthly", "Quarterly"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal PreviousPeriodRevenue { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }
        public List<SalesReportItemDto> SalesItems { get; set; } = new List<SalesReportItemDto>();
        public List<TopStoreDto> TopStores { get; set; } = new List<TopStoreDto>();
    }
}
