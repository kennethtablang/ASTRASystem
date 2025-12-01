using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.Enum;
using ASTRASystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IExcelService _excelService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            ApplicationDbContext context,
            IExcelService excelService,
            ILogger<ReportService> logger)
        {
            _context = context;
            _excelService = excelService;
            _logger = logger;
        }

        public async Task<byte[]> GenerateDailySalesReportAsync(DateTime date)
        {
            try
            {
                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.CreatedAt >= startOfDay && o.CreatedAt < endOfDay)
                    .AsNoTracking()
                    .ToListAsync();

                return _excelService.ExportOrdersToExcel(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily sales report");
                throw;
            }
        }

        public async Task<byte[]> GenerateDeliveryPerformanceReportAsync(DateTime from, DateTime to)
        {
            try
            {
                // Get all delivered orders in date range
                var deliveredOrders = await _context.Orders
                    .Include(o => o.Store)
                    .Where(o => o.Status == OrderStatus.Delivered &&
                               o.UpdatedAt >= from && o.UpdatedAt <= to)
                    .AsNoTracking()
                    .ToListAsync();

                // Calculate performance metrics
                // In a real system, you'd have delivery timestamps to calculate on-time delivery
                // For now, we'll export the delivered orders

                return _excelService.ExportOrdersToExcel(deliveredOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating delivery performance report");
                throw;
            }
        }

        public async Task<byte[]> GenerateAgentActivityReportAsync(string agentId, DateTime from, DateTime to)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                    .Where(o => o.AgentId == agentId &&
                               o.CreatedAt >= from && o.CreatedAt <= to)
                    .AsNoTracking()
                    .ToListAsync();

                return _excelService.ExportOrdersToExcel(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating agent activity report");
                throw;
            }
        }

        public async Task<byte[]> GenerateStockMovementReportAsync(long warehouseId, DateTime from, DateTime to)
        {
            try
            {
                // Get all orders from this warehouse in date range
                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.WarehouseId == warehouseId &&
                               o.CreatedAt >= from && o.CreatedAt <= to)
                    .AsNoTracking()
                    .ToListAsync();

                return _excelService.ExportOrdersToExcel(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating stock movement report");
                throw;
            }
        }

        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? DateTime.Today;
                var toDate = to ?? DateTime.Today.AddDays(1);

                var ordersQuery = _context.Orders.AsNoTracking();

                if (from.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt >= fromDate);
                }

                if (to.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt < toDate);
                }

                var stats = new DashboardStatsDto
                {
                    TotalOrders = await ordersQuery.CountAsync(),
                    PendingOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Pending),
                    ActiveTrips = await _context.Trips
                        .CountAsync(t => t.Status == TripStatus.Started || t.Status == TripStatus.InProgress),
                    DeliveredToday = await _context.Orders
                        .CountAsync(o => o.Status == OrderStatus.Delivered &&
                                        o.UpdatedAt >= DateTime.Today &&
                                        o.UpdatedAt < DateTime.Today.AddDays(1)),
                    TotalRevenue = await ordersQuery
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .SumAsync(o => o.Total),
                    ActiveStores = await _context.Stores.CountAsync()
                };

                // Calculate outstanding AR
                var invoices = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Payments)
                    .AsNoTracking()
                    .ToListAsync();

                decimal totalOutstanding = 0;
                foreach (var invoice in invoices)
                {
                    var totalPaid = invoice.Order.Payments.Sum(p => p.Amount);
                    var outstanding = invoice.TotalAmount - totalPaid;
                    if (outstanding > 0)
                    {
                        totalOutstanding += outstanding;
                    }
                }

                stats.OutstandingAR = totalOutstanding;

                // Calculate on-time delivery rate (simplified)
                var deliveredInRange = await ordersQuery
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .CountAsync();

                stats.OnTimeDeliveryRate = deliveredInRange > 0
                    ? (double)deliveredInRange / stats.TotalOrders * 100
                    : 0;

                return ApiResponse<DashboardStatsDto>.SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return ApiResponse<DashboardStatsDto>.ErrorResponse("An error occurred");
            }
        }
    }
}
