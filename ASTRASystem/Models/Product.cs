using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Product : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Sku { get; set; }

        [Required, MaxLength(300)]
        public string Name { get; set; }

        // Changed from string to foreign key relationship
        public long? CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string UnitOfMeasure { get; set; }

        public bool IsPerishable { get; set; } = false;
        public bool IsBarcoded { get; set; } = false;

        [MaxLength(100)]
        public string? Barcode { get; set; }
    }
}
