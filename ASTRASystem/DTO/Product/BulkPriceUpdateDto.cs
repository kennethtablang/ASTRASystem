using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Product
{
    public class BulkPriceUpdateDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one product must be specified")]
        public List<ProductPriceUpdateDto> Products { get; set; } = new();
    }
}
