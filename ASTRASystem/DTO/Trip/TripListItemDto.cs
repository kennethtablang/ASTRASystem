using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Trip
{
    public class TripListItemDto
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public string? DispatcherName { get; set; }
        public TripStatus Status { get; set; }
        public DateTime? DepartureAt { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
