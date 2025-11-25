using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.Delivery
{
    public class UploadDeliveryPhotoDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public IFormFile Photo { get; set; }

        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
