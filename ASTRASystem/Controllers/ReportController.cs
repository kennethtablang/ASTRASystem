using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASTRASystem.Controllers
{
    [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _reportService.GetDashboardStatsAsync(from, to);
            return Ok(result);
        }

        [HttpGet("daily-sales")]
        public async Task<IActionResult> GenerateDailySalesReport([FromQuery] DateTime date)
        {
            try
            {
                var excelData = await _reportService.GenerateDailySalesReportAsync(date);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"daily_sales_{date:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily sales report");
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        [HttpGet("delivery-performance")]
        public async Task<IActionResult> GenerateDeliveryPerformanceReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                var excelData = await _reportService.GenerateDeliveryPerformanceReportAsync(from, to);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"delivery_performance_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating delivery performance report");
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        [HttpGet("agent-activity")]
        public async Task<IActionResult> GenerateAgentActivityReport(
            [FromQuery] string agentId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                var excelData = await _reportService.GenerateAgentActivityReportAsync(agentId, from, to);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"agent_activity_{agentId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating agent activity report");
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        [HttpGet("stock-movement")]
        public async Task<IActionResult> GenerateStockMovementReport(
            [FromQuery] long warehouseId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                var excelData = await _reportService.GenerateStockMovementReportAsync(warehouseId, from, to);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"stock_movement_{warehouseId}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating stock movement report");
                return StatusCode(500, "An error occurred while generating the report");
            }
        }
    }
}
