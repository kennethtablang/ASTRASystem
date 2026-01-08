using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.Models
{
    public class Distributor : BaseEntity
    {
        [Required, MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string ContactPhone { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(250)]
        [EmailAddress]
        public string Email { get; set; }

        public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    }
}
