using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Trip
{
    public class TripAssignmentDto
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string StoreName { get; set; }
        public string? StoreBarangay { get; set; }
        public string? StoreCity { get; set; }
        public int SequenceNo { get; set; }
        public OrderStatus Status { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
