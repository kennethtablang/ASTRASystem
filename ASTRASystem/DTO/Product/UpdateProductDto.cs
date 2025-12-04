using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Product
{
    public class UpdateProductDto
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Sku { get; set; }

        [Required]
        [MaxLength(300)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string? UnitOfMeasure { get; set; }

        public bool IsPerishable { get; set; }

        public bool IsBarcoded { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }
    }
}
