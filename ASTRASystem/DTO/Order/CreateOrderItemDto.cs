using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class CreateOrderItemDto
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // Unit price can be overridden (e.g., for special pricing)
        public decimal? UnitPrice { get; set; }
    }
}
