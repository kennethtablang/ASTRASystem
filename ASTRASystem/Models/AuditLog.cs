using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.Models
{
    public class AuditLog : BaseEntity
    {
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required, MaxLength(250)]
        public string Action { get; set; }

        public string Meta { get; set; } // JSON blob or structured text

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
