namespace ASTRASystem.DTO.Reports
{
    public class DeliveryAgentPerformanceDto
    {
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public int TotalDeliveries { get; set; }
        public int OnTimeDeliveries { get; set; }
        public decimal OnTimePercentage { get; set; }
        public double AverageDeliveryTimeHours { get; set; }
    }
}
