using DocumentFormat.OpenXml.Drawing.Charts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Store : BaseEntity
    {
        [Required, MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Barangay { get; set; }

        [MaxLength(200)]
        public string City { get; set; }

        [MaxLength(200)]
        public string OwnerName { get; set; }

        [MaxLength(100)]
        public string Phone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; } = 0m;

        [MaxLength(100)]
        public string PreferredPaymentMethod { get; set; }

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
