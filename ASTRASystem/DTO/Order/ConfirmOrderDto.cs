using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class ConfirmOrderDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public long WarehouseId { get; set; }

        public List<OrderItemAdjustmentDto>? ItemAdjustments { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
