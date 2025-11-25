using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Delivery
{
    public class MarkDeliveredDto
    {
        [Required]
        public long OrderId { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<IFormFile>? Photos { get; set; }

        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
    }
}
