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

        /// <summary>
        /// Optionally point to the identity user who created the record.
        /// Type is left as object to avoid namespace issues; replace with ApplicationUser in your project.
        /// </summary>
        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }
    }
}
