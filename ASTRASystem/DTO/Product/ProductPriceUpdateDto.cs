using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Product
{
    public class ProductPriceUpdateDto
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal NewPrice { get; set; }

        [MaxLength(200)]
        public string? Reason { get; set; }
    }
}
