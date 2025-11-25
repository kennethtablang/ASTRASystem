namespace ASTRASystem.DTO.Product
{
    public class ProductDto
    {
        public long Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool IsPerishable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
