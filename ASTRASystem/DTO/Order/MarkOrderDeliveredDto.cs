using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class MarkOrderDeliveredDto
    {
        [Required]
        public long OrderId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
