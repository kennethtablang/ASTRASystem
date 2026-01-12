using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard-stats")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            long? distributorId = null;
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    distributorId = userDistributorId;
                }
            }

            var result = await _reportService.GetDashboardStatsAsync(from, to, distributorId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get top selling products
        /// </summary>
        [HttpGet("top-products")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetTopSellingProducts([FromQuery] int limit = 5, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var result = await _reportService.GetTopSellingProductsAsync(limit, from, to);
            return Ok(result);
        }



        /// <summary>
        /// Get weekly sales report with daily breakdown
        /// </summary>
        [HttpGet("sales/weekly")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetWeeklySalesReport([FromQuery] DateTime date, [FromQuery] long? distributorId = null)
        {
            var result = await _reportService.GetWeeklySalesReportAsync(date, distributorId);
            return Ok(result);
        }

        /// <summary>
        /// Generate daily sales report (Excel)
        /// </summary>
        [HttpGet("daily-sales")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GenerateDailySalesReport([FromQuery] DateTime date)
        {
            try
            {
                var excelBytes = await _reportService.GenerateDailySalesReportAsync(date);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DailySales_{date:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily sales report");
                return BadRequest(new { success = false, message = "Failed to generate report" });
            }
        }

        /// <summary>
        /// Generate delivery performance report (Excel)
        /// </summary>
        [HttpGet("delivery-performance")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GenerateDeliveryPerformanceReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                var excelBytes = await _reportService.GenerateDeliveryPerformanceReportAsync(from, to);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DeliveryPerformance_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating delivery performance report");
                return BadRequest(new { success = false, message = "Failed to generate report" });
            }
        }

        /// <summary>
        /// Generate agent activity report (Excel)
        /// </summary>
        [HttpGet("agent-activity/{agentId}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GenerateAgentActivityReport(
            string agentId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                var excelBytes = await _reportService.GenerateAgentActivityReportAsync(agentId, from, to);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"AgentActivity_{agentId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating agent activity report");
                return BadRequest(new { success = false, message = "Failed to generate report" });
            }
        }

        /// <summary>
        /// Generate stock movement report (Excel)
        /// </summary>
        [HttpGet("stock-movement/{warehouseId}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GenerateStockMovementReport(
            long warehouseId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                var excelBytes = await _reportService.GenerateStockMovementReportAsync(warehouseId, from, to);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"StockMovement_{warehouseId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating stock movement report");
                return BadRequest(new { success = false, message = "Failed to generate report" });
            }
        }

        /// <summary>
        /// Get daily sales report with detailed breakdown
        /// </summary>
        [HttpGet("sales/daily")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetDailySalesReport([FromQuery] DateTime? date, [FromQuery] long? distributorId = null)
        {
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    distributorId = userDistributorId;
                }
            }

            var reportDate = date ?? DateTime.Today;
            var result = await _reportService.GetDailySalesReportAsync(reportDate, distributorId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get monthly sales report with daily breakdown
        /// </summary>
        [HttpGet("sales/monthly")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetMonthlySalesReport([FromQuery] int? year, [FromQuery] int? month)
        {
            long? distributorId = null;
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    distributorId = userDistributorId;
                }
            }

            var reportYear = year ?? DateTime.Today.Year;
            var reportMonth = month ?? DateTime.Today.Month;
            var result = await _reportService.GetMonthlySalesReportAsync(reportYear, reportMonth, distributorId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get quarterly sales report with monthly breakdown
        /// </summary>
        [HttpGet("sales/quarterly")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetQuarterlySalesReport([FromQuery] int? year, [FromQuery] int? quarter)
        {
            long? distributorId = null;
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    distributorId = userDistributorId;
                }
            }

            var reportYear = year ?? DateTime.Today.Year;
            var reportQuarter = quarter ?? ((DateTime.Today.Month - 1) / 3 + 1);
            var result = await _reportService.GetQuarterlySalesReportAsync(reportYear, reportQuarter, distributorId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get delivery performance analytics with agent breakdown
        /// </summary>
        [HttpGet("delivery-performance-data")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetDeliveryPerformanceData(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            long? distributorId = null;
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    distributorId = userDistributorId;
                }
            }

            var startDate = from ?? DateTime.Today.AddDays(-30);
            var endDate = to ?? DateTime.Today;
            var result = await _reportService.GetDeliveryPerformanceDataAsync(startDate, endDate, distributorId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get fast moving products grouped by category
        /// </summary>
        [HttpGet("fast-moving-products")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetFastMovingProducts(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int limit = 5)
        {
            long? distributorId = null;
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    distributorId = userDistributorId;
                }
            }

            var startDate = from ?? DateTime.Today.AddDays(-30);
            var endDate = to ?? DateTime.Today;
            var result = await _reportService.GetFastMovingProductsByCategoryAsync(startDate, endDate, distributorId, limit);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}