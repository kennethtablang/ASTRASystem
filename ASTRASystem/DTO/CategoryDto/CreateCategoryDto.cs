using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.CategoryDto
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
