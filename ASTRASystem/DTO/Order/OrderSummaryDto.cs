namespace ASTRASystem.DTO.Order
{
    public class OrderSummaryDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int PackedOrders { get; set; }
        public int DispatchedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
