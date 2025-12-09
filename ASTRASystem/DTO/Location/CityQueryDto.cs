namespace ASTRASystem.DTO.Location
{
    public class CityQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? Province { get; set; }
        public string? Region { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
    }
}
