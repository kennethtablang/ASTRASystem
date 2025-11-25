using ASTRASystem.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTRASystem.Models
{
    public class TripAssignment : BaseEntity
    {
        public long TripId { get; set; }
        [ForeignKey(nameof(TripId))]
        public Trip Trip { get; set; }

        public long OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        public int SequenceNo { get; set; } = 0;

        public OrderStatus Status { get; set; } = OrderStatus.Packed;
    }
}
