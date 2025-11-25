namespace ASTRASystem.DTO.Order
{
    public class OrderItemDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
