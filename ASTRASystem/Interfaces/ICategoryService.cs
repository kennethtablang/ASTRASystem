using ASTRASystem.DTO.CategoryDto;
using ASTRASystem.DTO.Common;

namespace ASTRASystem.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(long id);
        Task<ApiResponse<CategoryDto>> GetCategoryByNameAsync(string name);
        Task<ApiResponse<List<CategoryListItemDto>>> GetCategoriesAsync(string? searchTerm = null);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto request, string userId);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto request, string userId);
        Task<ApiResponse<bool>> DeleteCategoryAsync(long id);
        Task<ApiResponse<List<string>>> GetCategoryNamesAsync();
    }
}
