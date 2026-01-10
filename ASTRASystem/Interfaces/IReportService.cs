using ASTRASystem.DTO.Common;

namespace ASTRASystem.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateDailySalesReportAsync(DateTime date);
        Task<byte[]> GenerateDeliveryPerformanceReportAsync(DateTime from, DateTime to);
        Task<byte[]> GenerateAgentActivityReportAsync(string agentId, DateTime from, DateTime to);
        Task<byte[]> GenerateStockMovementReportAsync(long warehouseId, DateTime from, DateTime to);
        Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DateTime? from = null, DateTime? to = null, long? distributorId = null);
        Task<ApiResponse<List<ASTRASystem.DTO.Reports.ProductSalesDto>>> GetTopSellingProductsAsync(int limit = 5, DateTime? from = null, DateTime? to = null);
        
        // New Sales Report Methods
        Task<ApiResponse<ASTRASystem.DTO.Reports.SalesReportDto>> GetDailySalesReportAsync(DateTime date, long? distributorId = null);
        Task<ApiResponse<ASTRASystem.DTO.Reports.SalesReportDto>> GetMonthlySalesReportAsync(int year, int month, long? distributorId = null);
        Task<ApiResponse<ASTRASystem.DTO.Reports.SalesReportDto>> GetQuarterlySalesReportAsync(int year, int quarter, long? distributorId = null);
        
        // New Delivery Performance Method
        Task<ApiResponse<ASTRASystem.DTO.Reports.DeliveryPerformanceDto>> GetDeliveryPerformanceDataAsync(DateTime from, DateTime to, long? distributorId = null);
        
        // New Fast Moving Products Method
        Task<ApiResponse<List<ASTRASystem.DTO.Reports.FastMovingProductsByCategoryDto>>> GetFastMovingProductsByCategoryAsync(DateTime from, DateTime to, long? distributorId = null, int topProductsPerCategory = 5);
    }
}
