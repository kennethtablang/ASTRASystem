namespace ASTRASystem.DTO.Product
{
    public class ProductQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public bool? IsPerishable { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
    }
}
