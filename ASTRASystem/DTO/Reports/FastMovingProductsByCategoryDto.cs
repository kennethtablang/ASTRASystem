namespace ASTRASystem.DTO.Reports
{
    public class FastMovingProductsByCategoryDto
    {
        public string CategoryName { get; set; }
        public decimal CategoryRevenue { get; set; }
        public int CategoryUnitsSold { get; set; }
        public decimal CategoryPercentage { get; set; }
        public List<ProductSalesDto> TopProducts { get; set; } = new List<ProductSalesDto>();
    }
}
