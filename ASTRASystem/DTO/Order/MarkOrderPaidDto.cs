using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class MarkOrderPaidDto
    {
        [Required]
        public long OrderId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
