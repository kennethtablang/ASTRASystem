namespace ASTRASystem.DTO.Reports
{
    public class DeliveryPerformanceDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDeliveries { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
        public decimal OnTimePercentage { get; set; }
        public double AverageDeliveryTimeHours { get; set; }
        public int PendingDeliveries { get; set; }
        public int InProgressDeliveries { get; set; }
        public List<DeliveryAgentPerformanceDto> AgentPerformance { get; set; } = new List<DeliveryAgentPerformanceDto>();
    }
}
