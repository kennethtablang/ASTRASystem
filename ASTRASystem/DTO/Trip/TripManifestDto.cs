namespace ASTRASystem.DTO.Trip
{
    public class TripManifestDto
    {
        public long TripId { get; set; }
        public string WarehouseName { get; set; }
        public string? WarehouseAddress { get; set; }
        public string DispatcherName { get; set; }
        public string? Vehicle { get; set; }
        public DateTime? DepartureAt { get; set; }
        public List<ManifestStopDto> Stops { get; set; } = new();
        public decimal TotalValue { get; set; }
        public int TotalOrders { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
