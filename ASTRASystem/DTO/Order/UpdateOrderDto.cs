using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class UpdateOrderDto
    {
        [Required]
        public long OrderId { get; set; }

        public bool Priority { get; set; } = false;

        public DateTime? ScheduledFor { get; set; }

        public long? DistributorId { get; set; }

        public long? WarehouseId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
