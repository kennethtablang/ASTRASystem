using DocumentFormat.OpenXml.Drawing.Charts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Warehouse : BaseEntity
    {
        [Required]
        public long DistributorId { get; set; }

        [ForeignKey(nameof(DistributorId))]
        public Distributor Distributor { get; set; }

        [Required, MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
