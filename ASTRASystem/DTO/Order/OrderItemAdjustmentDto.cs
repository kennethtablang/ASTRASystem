using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class OrderItemAdjustmentDto
    {
        [Required]
        public long OrderItemId { get; set; }

        [Range(0, int.MaxValue)]
        public int NewQuantity { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
