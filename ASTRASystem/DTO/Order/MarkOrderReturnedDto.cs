using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Order
{
    public class MarkOrderReturnedDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; }
    }
}
