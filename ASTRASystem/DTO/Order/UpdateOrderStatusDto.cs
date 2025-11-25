using ASTRASystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public OrderStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
