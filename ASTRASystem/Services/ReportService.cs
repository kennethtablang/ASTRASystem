using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO;
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

        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DateTime? from = null, DateTime? to = null, long? distributorId = null)
        {
            try
            {
                var fromDate = from ?? DateTime.Today;
                var toDate = to ?? DateTime.Today.AddDays(1);

                var ordersQuery = _context.Orders.AsNoTracking();

                // Filter by distributor
                if (distributorId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DistributorId == distributorId.Value);
                }

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
                    ActiveTrips = distributorId.HasValue 
                        ? await _context.Trips
                            .Include(t => t.Warehouse)
                            .CountAsync(t => (t.Status == TripStatus.Started || t.Status == TripStatus.InProgress) 
                                && t.Warehouse.DistributorId == distributorId.Value)
                        : await _context.Trips
                            .CountAsync(t => t.Status == TripStatus.Started || t.Status == TripStatus.InProgress),
                    DeliveredToday = await ordersQuery
                        .CountAsync(o => o.Status == OrderStatus.Delivered &&
                                        o.UpdatedAt >= DateTime.Today &&
                                        o.UpdatedAt < DateTime.Today.AddDays(1)),
                    TotalRevenue = await ordersQuery
                        .Where(o => o.Status == OrderStatus.Delivered)
                        .SumAsync(o => o.Total),
                    ActiveStores = distributorId.HasValue 
                        ? 0  // Stores don't have DistributorId currently
                        : await _context.Stores.CountAsync()
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
        public async Task<ApiResponse<List<DTO.Reports.ProductSalesDto>>> GetTopSellingProductsAsync(int limit = 5, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var query = _context.OrderItems
                    .Include(oi => oi.Order)
                    .AsNoTracking();

                if (from.HasValue)
                {
                    query = query.Where(oi => oi.Order.CreatedAt >= from.Value);
                }

                if (to.HasValue)
                {
                    query = query.Where(oi => oi.Order.CreatedAt < to.Value);
                }

                // Only count completed/valid orders if needed, for now all orders
                // query = query.Where(oi => oi.Order.Status != OrderStatus.Cancelled);

                var topProducts = await query
                    .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.Sku, oi.Product.Price, CategoryName = oi.Product.Category.Name })
                    .Select(g => new DTO.Reports.ProductSalesDto
                    {
                        Id = g.Key.ProductId,
                        Name = g.Key.Name,
                        Sku = g.Key.Sku,
                        Price = g.Key.Price,
                        CategoryName = g.Key.CategoryName,
                        UnitsSold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                    })
                    .OrderByDescending(p => p.UnitsSold)
                    .Take(limit)
                    .ToListAsync();

                return ApiResponse<List<DTO.Reports.ProductSalesDto>>.SuccessResponse(topProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling products");
                return ApiResponse<List<DTO.Reports.ProductSalesDto>>.ErrorResponse("An error occurred getting top products");
            }
        }

        public async Task<ApiResponse<DTO.Reports.SalesReportDto>> GetDailySalesReportAsync(DateTime date, long? distributorId = null)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);
                var previousStartDate = startDate.AddDays(-1);

                var ordersQuery = _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                    .AsNoTracking();

                if (distributorId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DistributorId == distributorId.Value);
                }

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                    .ToListAsync();

                var previousOrders = await ordersQuery
                    .Where(o => o.CreatedAt >= previousStartDate && o.CreatedAt < startDate)
                    .ToListAsync();

                var totalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);
                var previousRevenue = previousOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);

                var report = new DTO.Reports.SalesReportDto
                {
                    ReportType = "Daily",
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalRevenue = totalRevenue,
                    TotalOrders = orders.Count,
                    AverageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0,
                    PreviousPeriodRevenue = previousRevenue,
                    RevenueGrowthPercentage = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0,
                    SalesItems = new List<DTO.Reports.SalesReportItemDto>
                    {
                        new DTO.Reports.SalesReportItemDto
                        {
                            Date = startDate,
                            Revenue = totalRevenue,
                            OrderCount = orders.Count,
                            DeliveredOrderCount = orders.Count(o => o.Status == OrderStatus.Delivered),
                            PendingOrderCount = orders.Count(o => o.Status == OrderStatus.Pending),
                            AverageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0,
                            GrowthPercentage = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0
                        }
                    },
                    TopStores = orders
                        .GroupBy(o => new { o.StoreId, o.Store.Name })
                        .Select(g => new DTO.Reports.TopStoreDto
                        {
                            StoreId = g.Key.StoreId,
                            StoreName = g.Key.Name,
                            Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                            OrderCount = g.Count()
                        })
                        .OrderByDescending(s => s.Revenue)
                        .Take(5)
                        .ToList()
                };

                return ApiResponse<DTO.Reports.SalesReportDto>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily sales report");
                return ApiResponse<DTO.Reports.SalesReportDto>.ErrorResponse("An error occurred generating daily sales report");
            }
        }

        public async Task<ApiResponse<DTO.Reports.SalesReportDto>> GetWeeklySalesReportAsync(DateTime date, long? distributorId = null)
        {
            try
            {
                // Calculate start and end of week (Sunday to Saturday)
                var diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
                var startDate = date.AddDays(-1 * diff).Date;
                var endDate = startDate.AddDays(7);
                
                var previousStartDate = startDate.AddDays(-7);
                var previousEndDate = startDate;

                var ordersQuery = _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                    .AsNoTracking();

                if (distributorId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DistributorId == distributorId.Value);
                }

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                    .ToListAsync();

                var previousOrders = await ordersQuery
                    .Where(o => o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate)
                    .ToListAsync();

                var totalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);
                var previousRevenue = previousOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);

                // Group by day for weekly breakdown
                var dailyBreakdown = orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new DTO.Reports.SalesReportItemDto
                    {
                        Date = g.Key,
                        Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                        OrderCount = g.Count(),
                        DeliveredOrderCount = g.Count(o => o.Status == OrderStatus.Delivered),
                        PendingOrderCount = g.Count(o => o.Status == OrderStatus.Pending),
                        AverageOrderValue = g.Count() > 0 ? g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total) / g.Count() : 0,
                        GrowthPercentage = 0
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                var report = new DTO.Reports.SalesReportDto
                {
                    ReportType = "Weekly",
                    StartDate = startDate,
                    EndDate = endDate.AddDays(-1), // End date for display is inclusive
                    TotalRevenue = totalRevenue,
                    TotalOrders = orders.Count,
                    AverageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0,
                    PreviousPeriodRevenue = previousRevenue,
                    RevenueGrowthPercentage = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0,
                    SalesItems = dailyBreakdown,
                    TopStores = orders
                        .GroupBy(o => new { o.StoreId, o.Store.Name })
                        .Select(g => new DTO.Reports.TopStoreDto
                        {
                            StoreId = g.Key.StoreId,
                            StoreName = g.Key.Name,
                            Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                            OrderCount = g.Count()
                        })
                        .OrderByDescending(s => s.Revenue)
                        .Take(5)
                        .ToList()
                };

                return ApiResponse<DTO.Reports.SalesReportDto>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly sales report");
                return ApiResponse<DTO.Reports.SalesReportDto>.ErrorResponse("An error occurred generating weekly sales report");
            }
        }

        public async Task<ApiResponse<DTO.Reports.SalesReportDto>> GetMonthlySalesReportAsync(int year, int month, long? distributorId = null)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);
                var previousStartDate = startDate.AddMonths(-1);
                var previousEndDate = startDate;

                var ordersQuery = _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                    .AsNoTracking();

                if (distributorId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DistributorId == distributorId.Value);
                }

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                    .ToListAsync();

                var previousOrders = await ordersQuery
                    .Where(o => o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate)
                    .ToListAsync();

                var totalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);
                var previousRevenue = previousOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);

                // Group by day for monthly breakdown
                var dailySales = orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new DTO.Reports.SalesReportItemDto
                    {
                        Date = g.Key,
                        Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                        OrderCount = g.Count(),
                        DeliveredOrderCount = g.Count(o => o.Status == OrderStatus.Delivered),
                        PendingOrderCount = g.Count(o => o.Status == OrderStatus.Pending),
                        AverageOrderValue = g.Count() > 0 ? g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total) / g.Count() : 0,
                        GrowthPercentage = 0 // Can be calculated against same day previous month if needed
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                var report = new DTO.Reports.SalesReportDto
                {
                    ReportType = "Monthly",
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalRevenue = totalRevenue,
                    TotalOrders = orders.Count,
                    AverageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0,
                    PreviousPeriodRevenue = previousRevenue,
                    RevenueGrowthPercentage = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0,
                    SalesItems = dailySales,
                    TopStores = orders
                        .GroupBy(o => new { o.StoreId, o.Store.Name })
                        .Select(g => new DTO.Reports.TopStoreDto
                        {
                            StoreId = g.Key.StoreId,
                            StoreName = g.Key.Name,
                            Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                            OrderCount = g.Count()
                        })
                        .OrderByDescending(s => s.Revenue)
                        .Take(5)
                        .ToList()
                };

                return ApiResponse<DTO.Reports.SalesReportDto>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly sales report");
                return ApiResponse<DTO.Reports.SalesReportDto>.ErrorResponse("An error occurred generating monthly sales report");
            }
        }

        public async Task<ApiResponse<DTO.Reports.SalesReportDto>> GetQuarterlySalesReportAsync(int year, int quarter, long? distributorId = null)
        {
            try
            {
                if (quarter < 1 || quarter > 4)
                {
                    return ApiResponse<DTO.Reports.SalesReportDto>.ErrorResponse("Quarter must be between 1 and 4");
                }

                var startMonth = (quarter - 1) * 3 + 1;
                var startDate = new DateTime(year, startMonth, 1);
                var endDate = startDate.AddMonths(3);
                var previousStartDate = startDate.AddMonths(-3);
                var previousEndDate = startDate;

                var ordersQuery = _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                    .AsNoTracking();

                if (distributorId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DistributorId == distributorId.Value);
                }

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
                    .ToListAsync();

                var previousOrders = await ordersQuery
                    .Where(o => o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate)
                    .ToListAsync();

                var totalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);
                var previousRevenue = previousOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);

                // Group by month for quarterly breakdown
                var monthlySales = orders
                    .GroupBy(o => new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1))
                    .Select(g => new DTO.Reports.SalesReportItemDto
                    {
                        Date = g.Key,
                        Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                        OrderCount = g.Count(),
                        DeliveredOrderCount = g.Count(o => o.Status == OrderStatus.Delivered),
                        PendingOrderCount = g.Count(o => o.Status == OrderStatus.Pending),
                        AverageOrderValue = g.Count() > 0 ? g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total) / g.Count() : 0,
                        GrowthPercentage = 0
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                var report = new DTO.Reports.SalesReportDto
                {
                    ReportType = "Quarterly",
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalRevenue = totalRevenue,
                    TotalOrders = orders.Count,
                    AverageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0,
                    PreviousPeriodRevenue = previousRevenue,
                    RevenueGrowthPercentage = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0,
                    SalesItems = monthlySales,
                    TopStores = orders
                        .GroupBy(o => new { o.StoreId, o.Store.Name })
                        .Select(g => new DTO.Reports.TopStoreDto
                        {
                            StoreId = g.Key.StoreId,
                            StoreName = g.Key.Name,
                            Revenue = g.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
                            OrderCount = g.Count()
                        })
                        .OrderByDescending(s => s.Revenue)
                        .Take(5)
                        .ToList()
                };

                return ApiResponse<DTO.Reports.SalesReportDto>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quarterly sales report");
                return ApiResponse<DTO.Reports.SalesReportDto>.ErrorResponse("An error occurred generating quarterly sales report");
            }
        }

        public async Task<ApiResponse<DTO.Reports.DeliveryPerformanceDto>> GetDeliveryPerformanceDataAsync(DateTime from, DateTime to, long? distributorId = null)
        {
            try
            {
                var ordersQuery = _context.Orders
                    .Include(o => o.Store)
                    .AsNoTracking();

                if (distributorId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DistributorId == distributorId.Value);
                }

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                    .ToListAsync();

                var deliveredOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
                
                // Calculate on-time delivery (simplified: assume 24 hours is expected delivery time)
                var onTimeDeliveries = deliveredOrders.Where(o => 
                    o.UpdatedAt.HasValue && 
                    (o.UpdatedAt.Value - o.CreatedAt).TotalHours <= 24).Count();

                var totalDeliveries = deliveredOrders.Count;
                var averageDeliveryTime = deliveredOrders
                    .Where(o => o.UpdatedAt.HasValue)
                    .Select(o => (o.UpdatedAt.Value - o.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average();

                // Get agent performance - materialize data first to avoid EF translation issues
                var allAgentOrders = await _context.Orders
                    .Where(o => o.CreatedAt >= from && o.CreatedAt <= to && o.AgentId != null)
                    .Where(o => !distributorId.HasValue || o.DistributorId == distributorId.Value)
                    .Select(o => new
                    {
                        o.AgentId,
                        o.Status,
                        o.CreatedAt,
                        o.UpdatedAt
                    })
                    .ToListAsync();

                var agentPerformance = allAgentOrders
                    .GroupBy(o => o.AgentId)
                    .Select(g => new
                    {
                        AgentId = g.Key,
                        TotalDeliveries = g.Count(o => o.Status == OrderStatus.Delivered),
                        OnTimeDeliveries = g.Count(o => o.Status == OrderStatus.Delivered && 
                            o.UpdatedAt.HasValue && 
                            (o.UpdatedAt.Value - o.CreatedAt).TotalHours <= 24),
                        AvgDeliveryTime = g.Where(o => o.Status == OrderStatus.Delivered && o.UpdatedAt.HasValue)
                            .Select(o => (o.UpdatedAt.Value - o.CreatedAt).TotalHours)
                            .DefaultIfEmpty(0)
                            .Average()
                    })
                    .ToList();

                var report = new DTO.Reports.DeliveryPerformanceDto
                {
                    StartDate = from,
                    EndDate = to,
                    TotalDeliveries = totalDeliveries,
                    OnTimeDeliveries = onTimeDeliveries,
                    LateDeliveries = totalDeliveries - onTimeDeliveries,
                    OnTimePercentage = totalDeliveries > 0 ? (decimal)onTimeDeliveries / totalDeliveries * 100 : 0,
                    AverageDeliveryTimeHours = averageDeliveryTime,
                    PendingDeliveries = orders.Count(o => o.Status == OrderStatus.Pending),
                    InProgressDeliveries = orders.Count(o => o.Status == OrderStatus.InTransit || o.Status == OrderStatus.Dispatched || o.Status == OrderStatus.AtStore),
                    AgentPerformance = agentPerformance.Select(a => new DTO.Reports.DeliveryAgentPerformanceDto
                    {
                        AgentId = a.AgentId ?? "Unknown",
                        AgentName = a.AgentId ?? "Unknown Agent", // In a real system, lookup agent name from Users table
                        TotalDeliveries = a.TotalDeliveries,
                        OnTimeDeliveries = a.OnTimeDeliveries,
                        OnTimePercentage = a.TotalDeliveries > 0 ? (decimal)a.OnTimeDeliveries / a.TotalDeliveries * 100 : 0,
                        AverageDeliveryTimeHours = a.AvgDeliveryTime
                    }).ToList()
                };

                return ApiResponse<DTO.Reports.DeliveryPerformanceDto>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery performance data");
                return ApiResponse<DTO.Reports.DeliveryPerformanceDto>.ErrorResponse("An error occurred getting delivery performance data");
            }
        }

        public async Task<ApiResponse<List<DTO.Reports.FastMovingProductsByCategoryDto>>> GetFastMovingProductsByCategoryAsync(
            DateTime from, DateTime to, long? distributorId = null, int topProductsPerCategory = 5)
        {
            try
            {
                var orderItemsQuery = _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                    .AsNoTracking();

                if (distributorId.HasValue)
                {
                    orderItemsQuery = orderItemsQuery.Where(oi => oi.Order.DistributorId == distributorId.Value);
                }

                var orderItems = await orderItemsQuery
                    .Where(oi => oi.Order.CreatedAt >= from && oi.Order.CreatedAt <= to)
                    .Where(oi => oi.Product.Category != null) // Only include products with categories
                    .ToListAsync();

                // Calculate total revenue for percentage calculation
                var totalRevenue = orderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

                // Group by category
                var categorySales = orderItems
                    .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
                    .Select(categoryGroup => new DTO.Reports.FastMovingProductsByCategoryDto
                    {
                        CategoryName = categoryGroup.Key.Name,
                        CategoryRevenue = categoryGroup.Sum(oi => oi.Quantity * oi.UnitPrice),
                        CategoryUnitsSold = categoryGroup.Sum(oi => oi.Quantity),
                        CategoryPercentage = totalRevenue > 0 
                            ? (categoryGroup.Sum(oi => oi.Quantity * oi.UnitPrice) / totalRevenue) * 100 
                            : 0,
                        TopProducts = categoryGroup
                            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.Sku, oi.Product.Price })
                            .Select(productGroup => new DTO.Reports.ProductSalesDto
                            {
                                Id = productGroup.Key.ProductId,
                                Name = productGroup.Key.Name,
                                Sku = productGroup.Key.Sku,
                                Price = productGroup.Key.Price,
                                CategoryName = categoryGroup.Key.Name,
                                UnitsSold = productGroup.Sum(oi => oi.Quantity),
                                TotalRevenue = productGroup.Sum(oi => oi.Quantity * oi.UnitPrice)
                            })
                            .OrderByDescending(p => p.UnitsSold)
                            .Take(topProductsPerCategory)
                            .ToList()
                    })
                    .OrderByDescending(c => c.CategoryRevenue)
                    .ToList();

                return ApiResponse<List<DTO.Reports.FastMovingProductsByCategoryDto>>.SuccessResponse(categorySales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fast moving products by category");
                return ApiResponse<List<DTO.Reports.FastMovingProductsByCategoryDto>>.ErrorResponse("An error occurred getting fast moving products");
            }
        }
    }
}
