using Microsoft.EntityFrameworkCore;
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Product;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly IBarcodeService _barcodeService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            IBarcodeService barcodeService,
            IFileStorageService fileStorageService,
            ILogger<ProductService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _barcodeService = barcodeService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(long id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found");
                }

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResponse(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by ID {Id}", id);
                return ApiResponse<ProductDto>.ErrorResponse("An error occurred while retrieving product");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductBySkuAsync(string sku)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Sku == sku);

                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found");
                }

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResponse(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by SKU {Sku}", sku);
                return ApiResponse<ProductDto>.ErrorResponse("An error occurred while retrieving product");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByBarcodeAsync(string barcode)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Barcode == barcode);

                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found with this barcode");
                }

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResponse(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by barcode {Barcode}", barcode);
                return ApiResponse<ProductDto>.ErrorResponse("An error occurred while retrieving product");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(ProductQueryDto query)
        {
            try
            {
                var productsQuery = _context.Products
                    .Include(p => p.Category)
                    .AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchLower = query.SearchTerm.ToLower();
                    productsQuery = productsQuery.Where(p =>
                        p.Name.ToLower().Contains(searchLower) ||
                        p.Sku.ToLower().Contains(searchLower) ||
                        (p.Category != null && p.Category.Name.ToLower().Contains(searchLower)) ||
                        (p.Barcode != null && p.Barcode.ToLower().Contains(searchLower)));
                }

                if (query.CategoryId.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
                }

                if (query.IsPerishable.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.IsPerishable == query.IsPerishable.Value);
                }

                if (query.MinPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
                }

                if (query.MaxPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
                }

                // Apply sorting
                productsQuery = query.SortBy.ToLower() switch
                {
                    "name" => query.SortDescending
                        ? productsQuery.OrderByDescending(p => p.Name)
                        : productsQuery.OrderBy(p => p.Name),
                    "sku" => query.SortDescending
                        ? productsQuery.OrderByDescending(p => p.Sku)
                        : productsQuery.OrderBy(p => p.Sku),
                    "price" => query.SortDescending
                        ? productsQuery.OrderByDescending(p => p.Price)
                        : productsQuery.OrderBy(p => p.Price),
                    "category" => query.SortDescending
                        ? productsQuery.OrderByDescending(p => p.Category.Name)
                        : productsQuery.OrderBy(p => p.Category.Name),
                    _ => productsQuery.OrderBy(p => p.Name)
                };

                var totalCount = await productsQuery.CountAsync();
                var products = await productsQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name,
                    Price = p.Price,
                    UnitOfMeasure = p.UnitOfMeasure,
                    IsPerishable = p.IsPerishable,
                    IsBarcoded = p.IsBarcoded,
                    Barcode = p.Barcode,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                var paginatedResponse = new PaginatedResponse<ProductDto>(
                    productDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<ProductDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return ApiResponse<PaginatedResponse<ProductDto>>.ErrorResponse("An error occurred while retrieving products");
            }
        }


        public async Task<ApiResponse<List<ProductListItemDto>>> GetProductsForLookupAsync(string? searchTerm = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(searchLower) ||
                        p.Sku.ToLower().Contains(searchLower) ||
                        (p.Barcode != null && p.Barcode.ToLower().Contains(searchLower)));
                }

                var products = await query
                    .OrderBy(p => p.Name)
                    .Take(100) // Limit for lookup
                    .ToListAsync();

                var productDtos = _mapper.Map<List<ProductListItemDto>>(products);
                return ApiResponse<List<ProductListItemDto>>.SuccessResponse(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for lookup");
                return ApiResponse<List<ProductListItemDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto request, IFormFile? image, string userId)
        {
            try
            {
                // Check if SKU already exists
                var existingSku = await _context.Products
                    .AnyAsync(p => p.Sku.ToLower() == request.Sku.ToLower());

                if (existingSku)
                {
                    return ApiResponse<ProductDto>.ErrorResponse(
                        "A product with this SKU already exists");
                }

                // Validate category if provided
                if (request.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.Id == request.CategoryId.Value && c.IsActive);

                    if (!categoryExists)
                    {
                        return ApiResponse<ProductDto>.ErrorResponse("Invalid category selected");
                    }
                }

                // Validate barcode if product is barcoded
                if (request.IsBarcoded)
                {
                    if (string.IsNullOrWhiteSpace(request.Barcode))
                    {
                        return ApiResponse<ProductDto>.ErrorResponse(
                            "Barcode is required for barcoded products");
                    }

                    var existingBarcode = await _context.Products
                        .AnyAsync(p => p.Barcode != null && p.Barcode.ToLower() == request.Barcode.ToLower());

                    if (existingBarcode)
                    {
                        return ApiResponse<ProductDto>.ErrorResponse(
                            "A product with this barcode already exists");
                    }
                }

                var product = _mapper.Map<Product>(request);
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                product.CreatedById = userId;
                product.UpdatedById = userId;

                // Handle image upload if provided
                if (image != null)
                {
                    var imageUrl = await HandleImageUploadAsync(image);
                    product.ImageUrl = imageUrl;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Load category for response
                await _context.Entry(product).Reference(p => p.Category).LoadAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Product created",
                    new
                    {
                        ProductId = product.Id,
                        Sku = product.Sku,
                        Name = product.Name,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category?.Name
                    });

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Sku = product.Sku,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name,
                    Price = product.Price,
                    UnitOfMeasure = product.UnitOfMeasure,
                    IsPerishable = product.IsPerishable,
                    IsBarcoded = product.IsBarcoded,
                    Barcode = product.Barcode,
                    ImageUrl = product.ImageUrl,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                return ApiResponse<ProductDto>.SuccessResponse(
                    productDto,
                    "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return ApiResponse<ProductDto>.ErrorResponse("An error occurred while creating product");
            }
        }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto request, IFormFile? image, bool removeImage, string userId)
        {
            try
            {
                var product = await _context.Products.FindAsync(request.Id);
                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found");
                }

                // Check if SKU already exists (excluding current product)
                var duplicateSku = await _context.Products
                    .AnyAsync(p => p.Sku.ToLower() == request.Sku.ToLower() && p.Id != request.Id);

                if (duplicateSku)
                {
                    return ApiResponse<ProductDto>.ErrorResponse(
                        "A product with this SKU already exists");
                }

                // Validate barcode if product is barcoded
                if (request.IsBarcoded)
                {
                    if (string.IsNullOrWhiteSpace(request.Barcode))
                    {
                        return ApiResponse<ProductDto>.ErrorResponse(
                            "Barcode is required for barcoded products");
                    }

                    // Check if barcode already exists (excluding current product)
                    var duplicateBarcode = await _context.Products
                        .AnyAsync(p => p.Barcode != null &&
                                      p.Barcode.ToLower() == request.Barcode.ToLower() &&
                                      p.Id != request.Id);

                    if (duplicateBarcode)
                    {
                        return ApiResponse<ProductDto>.ErrorResponse(
                            "A product with this barcode already exists");
                    }
                }
                else
                {
                    // Clear barcode if product is marked as non-barcoded
                    request.Barcode = null;
                }

                var oldPrice = product.Price;
                var oldBarcode = product.Barcode;
                var oldImageUrl = product.ImageUrl;

                product.Sku = request.Sku;
                product.Name = request.Name;
                product.CategoryId = request.CategoryId;
                product.Price = request.Price;
                product.UnitOfMeasure = request.UnitOfMeasure;
                product.IsPerishable = request.IsPerishable;
                product.IsBarcoded = request.IsBarcoded;
                product.Barcode = request.Barcode;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedById = userId;

                // Handle image operations
                if (removeImage && !string.IsNullOrEmpty(product.ImageUrl))
                {
                    // Delete existing image
                    await _fileStorageService.DeleteFileAsync(product.ImageUrl);
                    product.ImageUrl = null;
                }
                else if (image != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        await _fileStorageService.DeleteFileAsync(product.ImageUrl);
                    }
                    // Upload new image
                    var imageUrl = await HandleImageUploadAsync(image);
                    product.ImageUrl = imageUrl;
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Product updated",
                    new
                    {
                        ProductId = product.Id,
                        Sku = product.Sku,
                        Name = product.Name,
                        OldPrice = oldPrice,
                        NewPrice = request.Price,
                        IsBarcoded = product.IsBarcoded,
                        OldBarcode = oldBarcode,
                        NewBarcode = product.Barcode
                    });

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResponse(
                    productDto,
                    "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return ApiResponse<ProductDto>.ErrorResponse("An error occurred while updating product");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(long id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Product not found");
                }

                // Check if product is used in any orders
                var isUsedInOrders = await _context.OrderItems
                    .AnyAsync(oi => oi.ProductId == id);

                if (isUsedInOrders)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete product that has been used in orders",
                        new List<string> { "This product exists in order history and cannot be deleted." });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting product");
            }
        }

        public async Task<ApiResponse<bool>> BulkUpdatePricesAsync(BulkPriceUpdateDto request, string userId)
        {
            try
            {
                var productIds = request.Products.Select(p => p.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != request.Products.Count)
                {
                    return ApiResponse<bool>.ErrorResponse("Some products were not found");
                }

                var updates = new List<object>();

                foreach (var updateRequest in request.Products)
                {
                    var product = products.First(p => p.Id == updateRequest.ProductId);
                    var oldPrice = product.Price;

                    product.Price = updateRequest.NewPrice;
                    product.UpdatedAt = DateTime.UtcNow;
                    product.UpdatedById = userId;

                    updates.Add(new
                    {
                        ProductId = product.Id,
                        Sku = product.Sku,
                        Name = product.Name,
                        OldPrice = oldPrice,
                        NewPrice = updateRequest.NewPrice,
                        Reason = updateRequest.Reason
                    });
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Bulk price update",
                    new { UpdateCount = updates.Count, Updates = updates });

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    $"{updates.Count} product prices updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk price update");
                return ApiResponse<bool>.ErrorResponse("An error occurred while updating prices");
            }
        }

        public async Task<ApiResponse<List<string>>> GetProductCategoriesAsync()
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
                _logger.LogError(ex, "Error getting product categories");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<byte[]>> GenerateBarcodeAsync(GenerateBarcodeDto request)
        {
            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId);

                if (product == null)
                {
                    return ApiResponse<byte[]>.ErrorResponse("Product not found");
                }

                // Use barcode if available, otherwise use SKU
                var barcodeContent = !string.IsNullOrWhiteSpace(product.Barcode)
                    ? product.Barcode
                    : product.Sku;

                byte[] barcodeImage;

                if (request.Format.ToUpper() == "QR")
                {
                    barcodeImage = _barcodeService.GenerateQRCode(
                        barcodeContent,
                        request.Width,
                        request.Height);
                }
                else
                {
                    barcodeImage = _barcodeService.GenerateBarcode(
                        barcodeContent,
                        request.Width,
                        request.Height);
                }

                return ApiResponse<byte[]>.SuccessResponse(barcodeImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating barcode");
                return ApiResponse<byte[]>.ErrorResponse("An error occurred while generating barcode");
            }
        }

        public async Task<ApiResponse<ProductDto>> UploadProductImageAsync(long productId, IFormFile image, string userId)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found");
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _fileStorageService.DeleteFileAsync(product.ImageUrl);
                }

                // Upload new image
                var imageUrl = await HandleImageUploadAsync(image);
                product.ImageUrl = imageUrl;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Product image uploaded",
                    new
                    {
                        ProductId = product.Id,
                        Sku = product.Sku,
                        Name = product.Name,
                        ImageUrl = imageUrl
                    });

                var productDto = _mapper.Map<ProductDto>(product);
                return ApiResponse<ProductDto>.SuccessResponse(
                    productDto,
                    "Product image uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image");
                return ApiResponse<ProductDto>.ErrorResponse("An error occurred while uploading image");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductImageAsync(long productId, string userId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Product not found");
                }

                if (string.IsNullOrEmpty(product.ImageUrl))
                {
                    return ApiResponse<bool>.ErrorResponse("Product has no image to delete");
                }

                // Delete physical file
                await _fileStorageService.DeleteFileAsync(product.ImageUrl);

                // Update product
                product.ImageUrl = null;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Product image deleted",
                    new { ProductId = product.Id, Sku = product.Sku, Name = product.Name });

                return ApiResponse<bool>.SuccessResponse(true, "Product image deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image");
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting image");
            }
        }

        private async Task<string> HandleImageUploadAsync(IFormFile image)
        {
            // Validate image
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var maxSize = 5 * 1024 * 1024; // 5MB

            if (image.Length > maxSize)
            {
                throw new InvalidOperationException("Image size must not exceed 5MB");
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Invalid image format: {extension}. Allowed formats: JPG, JPEG, PNG, WEBP, GIF");
            }

            // Generate unique filename
            var fileName = $"product_{Guid.NewGuid()}{extension}";

            // Upload file
            using var stream = image.OpenReadStream();
            var imageUrl = await _fileStorageService.UploadFileAsync(stream, fileName, image.ContentType);

            return imageUrl;
        }
    }
}
