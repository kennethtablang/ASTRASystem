using ASTRASystem.Enum;

namespace ASTRASystem.DTO.Trip
{
    public class TripQueryDto
    {
        public TripStatus? Status { get; set; }
        public long? WarehouseId { get; set; }
        public string? DispatcherId { get; set; }
        public DateTime? DepartureFrom { get; set; }
        public DateTime? DepartureTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "DepartureAt";
        public bool SortDescending { get; set; } = true;
    }
}
