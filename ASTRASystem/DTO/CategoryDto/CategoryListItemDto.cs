namespace ASTRASystem.DTO.CategoryDto
{
    public class CategoryListItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
    }
}
