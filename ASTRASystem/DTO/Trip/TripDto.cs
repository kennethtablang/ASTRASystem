using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Trip
{
    public class TripDto
    {
        public long Id { get; set; }
        public long WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string? DispatcherId { get; set; }
        public string? DispatcherName { get; set; }
        public TripStatus Status { get; set; }
        public DateTime? DepartureAt { get; set; }
        public string? Vehicle { get; set; }
        public DateTime? EstimatedReturn { get; set; }
        public List<TripAssignmentDto> Assignments { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
