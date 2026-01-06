namespace ASTRASystem.DTO.Product
{
    public class ProductListItemDto
    {
        public long Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool IsBarcoded { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
    }
}
