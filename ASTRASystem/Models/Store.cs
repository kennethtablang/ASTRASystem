using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Store : BaseEntity
    {
        [Required, MaxLength(250)]
        public string Name { get; set; }

        // Foreign key relationships for location
        public long? BarangayId { get; set; }
        [ForeignKey(nameof(BarangayId))]
        public Barangay? Barangay { get; set; }

        public long? CityId { get; set; }
        [ForeignKey(nameof(CityId))]
        public City? City { get; set; }

        [MaxLength(200)]
        public string? OwnerName { get; set; }

        [MaxLength(100)]
        public string? Phone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; } = 0m;

        [MaxLength(100)]
        public string? PreferredPaymentMethod { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}