using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Barangay : BaseEntity
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public long CityId { get; set; }

        [ForeignKey(nameof(CityId))]
        public City City { get; set; }

        [MaxLength(100)]
        public string? ZipCode { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}
