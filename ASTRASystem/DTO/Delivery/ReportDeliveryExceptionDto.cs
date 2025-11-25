using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Delivery
{
    public class ReportDeliveryExceptionDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ExceptionType { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        public List<IFormFile>? Photos { get; set; }
    }
}
