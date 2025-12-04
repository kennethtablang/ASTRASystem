using ASTRASystem.Data;
using ASTRASystem.DTO.CategoryDto;
using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<CategoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(long id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                categoryDto.ProductCount = category.Products?.Count ?? 0;

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID {Id}", id);
                return ApiResponse<CategoryDto>.ErrorResponse("An error occurred while retrieving category");
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByNameAsync(string name)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

                if (category == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                categoryDto.ProductCount = category.Products?.Count ?? 0;

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by name {Name}", name);
                return ApiResponse<CategoryDto>.ErrorResponse("An error occurred while retrieving category");
            }
        }

        public async Task<ApiResponse<List<CategoryListItemDto>>> GetCategoriesAsync(string? searchTerm = null)
        {
            try
            {
                var query = _context.Categories
                    .Include(c => c.Products)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(c =>
                        c.Name.ToLower().Contains(searchLower) ||
                        (c.Description != null && c.Description.ToLower().Contains(searchLower)));
                }

                var categories = await query
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var categoryDtos = categories.Select(c =>
                {
                    var dto = _mapper.Map<CategoryListItemDto>(c);
                    dto.ProductCount = c.Products?.Count ?? 0;
                    return dto;
                }).ToList();

                return ApiResponse<List<CategoryListItemDto>>.SuccessResponse(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return ApiResponse<List<CategoryListItemDto>>.ErrorResponse("An error occurred while retrieving categories");
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto request, string userId)
        {
            try
            {
                // Check if category name already exists
                var existingCategory = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower());

                if (existingCategory)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse(
                        "A category with this name already exists");
                }

                var category = _mapper.Map<Category>(request);
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                category.CreatedById = userId;
                category.UpdatedById = userId;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Category created",
                    new
                    {
                        CategoryId = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        Color = category.Color
                    });

                var categoryDto = _mapper.Map<CategoryDto>(category);
                categoryDto.ProductCount = 0;

                return ApiResponse<CategoryDto>.SuccessResponse(
                    categoryDto,
                    "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return ApiResponse<CategoryDto>.ErrorResponse("An error occurred while creating category");
            }
        }

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto request, string userId)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == request.Id);

                if (category == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found");
                }

                // Check if new name already exists (excluding current category)
                var duplicateName = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.Id != request.Id);

                if (duplicateName)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse(
                        "A category with this name already exists");
                }

                var oldName = category.Name;

                category.Name = request.Name;
                category.Description = request.Description;
                category.Color = request.Color;
                category.IsActive = request.IsActive;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Category updated",
                    new
                    {
                        CategoryId = category.Id,
                        OldName = oldName,
                        NewName = request.Name,
                        Description = request.Description,
                        Color = request.Color
                    });

                var categoryDto = _mapper.Map<CategoryDto>(category);
                categoryDto.ProductCount = category.Products?.Count ?? 0;

                return ApiResponse<CategoryDto>.SuccessResponse(
                    categoryDto,
                    "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return ApiResponse<CategoryDto>.ErrorResponse("An error occurred while updating category");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(long id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Category not found");
                }

                // Check if category has products
                if (category.Products != null && category.Products.Any())
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete category that has products",
                        new List<string> { $"This category has {category.Products.Count} product(s). Please remove or reassign these products before deleting the category." });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting category");
            }
        }

        public async Task<ApiResponse<List<string>>> GetCategoryNamesAsync()
        {
            try
            {
                var categoryNames = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResponse(categoryNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category names");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred");
            }
        }
    }
}
