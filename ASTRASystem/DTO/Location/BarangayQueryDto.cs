namespace ASTRASystem.DTO.Location
{
    public class BarangayQueryDto
    {
        public string? SearchTerm { get; set; }
        public long? CityId { get; set; }
        public string? ZipCode { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
    }
}
