namespace ASTRASystem.DTO.Common
{
    public class WarehouseDto
    {
        public long Id { get; set; }
        public long DistributorId { get; set; }
        public string? DistributorName { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
