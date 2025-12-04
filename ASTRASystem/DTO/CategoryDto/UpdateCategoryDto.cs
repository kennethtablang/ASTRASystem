using System.ComponentModel.DataAnnotations;

namespace ASTRASystem.DTO.CategoryDto
{
    public class UpdateCategoryDto
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public bool IsActive { get; set; }
    }
}
