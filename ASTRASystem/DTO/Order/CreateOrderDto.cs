using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Store ID is required")]
        public long StoreId { get; set; }

        public long? DistributorId { get; set; }
        public long? WarehouseId { get; set; }

        public bool Priority { get; set; } = false;

        public DateTime? ScheduledFor { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
