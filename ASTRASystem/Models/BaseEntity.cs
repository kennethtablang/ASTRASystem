using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.Models
{
    public class BaseEntity
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }
    }
}
