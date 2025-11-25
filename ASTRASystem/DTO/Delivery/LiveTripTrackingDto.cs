namespace ASTRASystem.DTO.Delivery
{
    public class LiveTripTrackingDto
    {
        public long TripId { get; set; }
        public string DispatcherName { get; set; }
        public string? Vehicle { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public List<TripStopStatusDto> Stops { get; set; } = new();
        public int CompletedStops { get; set; }
        public int TotalStops { get; set; }
    }
}
