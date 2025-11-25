using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class DeliveryPhoto : BaseEntity
    {
        public long OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [Required, MaxLength(1000)]
        public string Url { get; set; }

        [MaxLength(450)]
        public string UploadedById { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }
    }
}
