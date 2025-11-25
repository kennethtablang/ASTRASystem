using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.Models
{
    public class Notification : BaseEntity
    {
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Type { get; set; }

        public string Payload { get; set; }

        public DateTime? SentAt { get; set; }

        public bool IsRead { get; set; } = false;
    }
}
