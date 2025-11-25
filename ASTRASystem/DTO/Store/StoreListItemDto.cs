namespace ASTRASystem.DTO.Store
{
    public class StoreListItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Barangay { get; set; }
        public string? City { get; set; }
        public string? OwnerName { get; set; }
        public string? Phone { get; set; }
    }
}
