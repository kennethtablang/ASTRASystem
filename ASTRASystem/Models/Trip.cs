using ASTRASystem.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class Trip : BaseEntity
    {
        public long WarehouseId { get; set; }
        [ForeignKey(nameof(WarehouseId))]
        public Warehouse Warehouse { get; set; }

        /// <summary>
        /// Dispatcher (rider) assigned to this trip.
        /// Stored as user id string to work with Identity's ApplicationUser.Id type.
        /// </summary>
        [MaxLength(450)]
        public string DispatcherId { get; set; }

        public TripStatus Status { get; set; } = TripStatus.Created;

        public DateTime? DepartureAt { get; set; }

        [MaxLength(200)]
        public string Vehicle { get; set; }

        // Optional estimated return/time window
        public DateTime? EstimatedReturn { get; set; }

        // Navigation
        public ICollection<TripAssignment> Assignments { get; set; } = new List<TripAssignment>();
    }
}
