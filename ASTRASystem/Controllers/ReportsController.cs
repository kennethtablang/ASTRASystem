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
        public async Task<IActionResult> GetDashboardStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _reportService.GetDashboardStatsAsync(from, to);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Generate daily sales report (Excel)
        /// </summary>
        [HttpGet("daily-sales")]
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
    }
}