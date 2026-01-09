using ASTRASystem.DTO.Product;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(long id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("sku/{sku}")]
        public async Task<IActionResult> GetProductBySku(string sku)
        {
            var result = await _productService.GetProductBySkuAsync(sku);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            try
            {
                var result = await _productService.GetProductByBarcodeAsync(barcode);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by barcode {Barcode}", barcode);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto query)
        {
            var result = await _productService.GetProductsAsync(query);
            return Ok(result);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetProductsForLookup([FromQuery] string? searchTerm = null)
        {
            var result = await _productService.GetProductsForLookupAsync(searchTerm);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto request, IFormFile? image)
        {
            // Use ClaimTypes.NameIdentifier for the user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateProduct: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CreateProduct: User {UserId} creating product {ProductSku}", userId, request.Sku);

            var result = await _productService.CreateProductAsync(request, image, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetProductById), new { id = result.Data.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateProduct(
            long id,
            [FromForm] UpdateProductDto request,
            IFormFile? image,
            [FromForm] bool removeImage = false)
        {
            if (id != request.Id)
            {
                return BadRequest(new { success = false, message = "ID mismatch" });
            }

            // Use ClaimTypes.NameIdentifier for the user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateProduct: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateProduct: User {UserId} updating product {ProductId}", userId, id);

            var result = await _productService.UpdateProductAsync(request, image, removeImage, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("bulk-price-update")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> BulkUpdatePrices([FromBody] BulkPriceUpdateDto request)
        {
            // Use ClaimTypes.NameIdentifier for the user ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("BulkUpdatePrices: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            var result = await _productService.BulkUpdatePricesAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _productService.GetProductCategoriesAsync();
            return Ok(result);
        }

        [HttpPost("{id}/image")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UploadProductImage(long id, IFormFile image)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            if (image == null)
            {
                return BadRequest(new { success = false, message = "Image file is required" });
            }

            var result = await _productService.UploadProductImageAsync(id, image, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}/image")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> DeleteProductImage(long id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            var result = await _productService.DeleteProductImageAsync(id, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("barcode")]
        public async Task<IActionResult> GenerateBarcode([FromBody] GenerateBarcodeDto request)
        {
            var result = await _productService.GenerateBarcodeAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return File(result.Data, "image/png", $"barcode_{request.ProductId}.png");
        }
    }
}