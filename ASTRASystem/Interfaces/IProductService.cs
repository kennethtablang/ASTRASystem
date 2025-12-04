using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Product;

namespace ASTRASystem.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(long id);
        Task<ApiResponse<ProductDto>> GetProductBySkuAsync(string sku);
        Task<ApiResponse<ProductDto>> GetProductByBarcodeAsync(string barcode);
        Task<ApiResponse<PaginatedResponse<ProductDto>>> GetProductsAsync(ProductQueryDto query);
        Task<ApiResponse<List<ProductListItemDto>>> GetProductsForLookupAsync(string? searchTerm = null);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto request, string userId);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto request, string userId);
        Task<ApiResponse<bool>> DeleteProductAsync(long id);
        Task<ApiResponse<bool>> BulkUpdatePricesAsync(BulkPriceUpdateDto request, string userId);
        Task<ApiResponse<List<string>>> GetProductCategoriesAsync();
        Task<ApiResponse<byte[]>> GenerateBarcodeAsync(GenerateBarcodeDto request);
    }

    public interface IBarcodeService
    {
        byte[] GenerateQRCode(string content, int width = 300, int height = 300);
        byte[] GenerateBarcode(string content, int width = 300, int height = 100);
    }
}
