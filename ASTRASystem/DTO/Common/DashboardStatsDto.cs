namespace ASTRASystem.DTO.Common
{
    public class DashboardStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ActiveTrips { get; set; }
        public int DeliveredToday { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal OutstandingAR { get; set; }
        public int ActiveStores { get; set; }
        public double OnTimeDeliveryRate { get; set; }
    }
}
