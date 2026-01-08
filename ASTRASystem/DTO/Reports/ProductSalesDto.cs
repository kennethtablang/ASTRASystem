namespace ASTRASystem.DTO.Reports
{
    public class ProductSalesDto
    {
        public long Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
