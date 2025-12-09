namespace ASTRASystem.DTO.Location
{
    public class CityListItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Province { get; set; }
        public string? Region { get; set; }
        public bool IsActive { get; set; }
        public int BarangayCount { get; set; }
        public int StoreCount { get; set; }
    }
}
