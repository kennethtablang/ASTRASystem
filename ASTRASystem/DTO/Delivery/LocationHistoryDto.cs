using System;

namespace ASTRASystem.DTO.Delivery
{
    public class LocationHistoryDto
    {
        public long TripId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Accuracy { get; set; }
        public DateTime Timestamp { get; set; }
        public string Event { get; set; }
    }
}
