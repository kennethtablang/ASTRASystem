using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Product
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "SKU is required")]
        [MaxLength(100)]
        public string Sku { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(300)]
        public string Name { get; set; }

        public long? CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999999999.99, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string? UnitOfMeasure { get; set; }

        public bool IsPerishable { get; set; } = false;

        public bool IsBarcoded { get; set; } = false;

        [MaxLength(100)]
        public string? Barcode { get; set; }
    }
}
