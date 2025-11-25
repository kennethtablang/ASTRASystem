using ASTRASystem.DTO.Common;

namespace ASTRASystem.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateDailySalesReportAsync(DateTime date);
        Task<byte[]> GenerateDeliveryPerformanceReportAsync(DateTime from, DateTime to);
        Task<byte[]> GenerateAgentActivityReportAsync(string agentId, DateTime from, DateTime to);
        Task<byte[]> GenerateStockMovementReportAsync(long warehouseId, DateTime from, DateTime to);
        Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DateTime? from = null, DateTime? to = null);
    }
}
