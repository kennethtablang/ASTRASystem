using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class BatchCreateOrderDto
    {
        [Required]
        [MinLength(1)]
        public List<CreateOrderDto> Orders { get; set; } = new();
    }
}
