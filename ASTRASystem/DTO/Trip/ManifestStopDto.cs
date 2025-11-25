namespace ASTRASystem.DTO.Trip
{
    public class ManifestStopDto
    {
        public int SequenceNo { get; set; }
        public long OrderId { get; set; }
        public string StoreName { get; set; }
        public string? StoreAddress { get; set; }
        public string? StorePhone { get; set; }
        public string? StoreOwner { get; set; }
        public decimal OrderTotal { get; set; }
        public List<ManifestItemDto> Items { get; set; } = new();
        public string? SpecialNotes { get; set; }
    }
}
