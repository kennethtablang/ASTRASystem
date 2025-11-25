namespace ASTRASystem.DTO.Store
{
    public class StoreQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? Barangay { get; set; }
        public string? City { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
    }
}
