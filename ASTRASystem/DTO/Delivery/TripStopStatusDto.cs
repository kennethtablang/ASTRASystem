namespace ASTRASystem.DTO.Delivery
{
    public class TripStopStatusDto
    {
        public long OrderId { get; set; }
        public int SequenceNo { get; set; }
        public string StoreName { get; set; }
        public string Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public bool HasException { get; set; }
    }
}
