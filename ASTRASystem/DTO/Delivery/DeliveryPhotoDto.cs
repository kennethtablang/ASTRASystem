namespace ASTRASystem.DTO.Delivery
{
    public class DeliveryPhotoDto
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string Url { get; set; }
        public string? UploadedById { get; set; }
        public string? UploadedByName { get; set; }
        public DateTime UploadedAt { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public string? Notes { get; set; }
    }
}
