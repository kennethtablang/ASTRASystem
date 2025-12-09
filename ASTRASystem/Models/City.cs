using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.Models
{
    public class City : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string? Province { get; set; }

        [MaxLength(200)]
        public string? Region { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Barangay> Barangays { get; set; } = new List<Barangay>();
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}
