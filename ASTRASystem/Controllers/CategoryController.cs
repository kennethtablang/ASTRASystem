using ASTRASystem.DTO.CategoryDto;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(long id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetCategoryByName(string name)
        {
            var result = await _categoryService.GetCategoryByNameAsync(name);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] string? searchTerm = null)
        {
            var result = await _categoryService.GetCategoriesAsync(searchTerm);
            return Ok(result);
        }

        [HttpGet("names")]
        public async Task<IActionResult> GetCategoryNames()
        {
            var result = await _categoryService.GetCategoryNamesAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto request)
        {
            // Use ClaimTypes.NameIdentifier for the user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateCategory: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CreateCategory: User {UserId} creating category {CategoryName}", userId, request.Name);

            var result = await _categoryService.CreateCategoryAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] UpdateCategoryDto request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { success = false, message = "ID mismatch" });
            }

            // Use ClaimTypes.NameIdentifier for the user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateCategory: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateCategory: User {UserId} updating category {CategoryId}", userId, id);

            var result = await _categoryService.UpdateCategoryAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}