namespace ASTRASystem.DTO.Location
{
    public class BarangayDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long CityId { get; set; }
        public string CityName { get; set; }
        public string? Province { get; set; }
        public string? ZipCode { get; set; }
        public bool IsActive { get; set; }
        public int StoreCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
