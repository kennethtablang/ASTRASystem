using ASTRASystem.Data;
using ASTRASystem.Interfaces;
using ASTRASystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController : ControllerBase
    {
        private readonly IThermalReceiptService _receiptService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReceiptController> _logger;

        public ReceiptController(
            IThermalReceiptService receiptService,
            ApplicationDbContext context,
            ILogger<ReceiptController> logger)
        {
            _receiptService = receiptService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Generate thermal receipt for mobile printer (58mm)
        /// Returns Base64 encoded ESC/POS commands
        /// </summary>
        [HttpGet("thermal/{orderId}/mobile")]
        public async Task<IActionResult> GenerateMobileThermalReceipt(long orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.Payments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }

                // Generate Base64 receipt for mobile transmission
                var base64Receipt = _receiptService.GenerateBase64Receipt(order, paperWidth: 58);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId = order.Id,
                        receiptData = base64Receipt,
                        paperWidth = 58,
                        encoding = "Windows-1252",
                        format = "ESC/POS"
                    },
                    message = "Mobile thermal receipt generated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating mobile thermal receipt for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Generate thermal receipt for desktop printer (80mm)
        /// Returns Base64 encoded ESC/POS commands
        /// </summary>
        [HttpGet("thermal/{orderId}/desktop")]
        public async Task<IActionResult> GenerateDesktopThermalReceipt(long orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.Payments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }

                // Generate Base64 receipt for desktop printer
                var base64Receipt = _receiptService.GenerateBase64Receipt(order, paperWidth: 80);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId = order.Id,
                        receiptData = base64Receipt,
                        paperWidth = 80,
                        encoding = "Windows-1252",
                        format = "ESC/POS"
                    },
                    message = "Desktop thermal receipt generated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating desktop thermal receipt for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Download thermal receipt as binary file
        /// Can be sent directly to printer
        /// </summary>
        [HttpGet("thermal/{orderId}/download")]
        public async Task<IActionResult> DownloadThermalReceipt(long orderId, [FromQuery] int paperWidth = 58)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.Payments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }

                // Generate receipt bytes
                var receiptBytes = _receiptService.GenerateReceiptBytes(order, paperWidth);

                return File(
                    receiptBytes,
                    "application/octet-stream",
                    $"receipt_{orderId}_{DateTime.Now:yyyyMMddHHmmss}.bin"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading thermal receipt for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Preview thermal receipt as plain text
        /// Useful for testing and debugging
        /// </summary>
        [HttpGet("thermal/{orderId}/preview")]
        public async Task<IActionResult> PreviewThermalReceipt(long orderId, [FromQuery] int paperWidth = 58)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.Payments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }

                // Generate receipt (contains ESC/POS commands)
                var receipt = _receiptService.GenerateThermalReceipt(order, paperWidth);

                // Strip ESC/POS commands for preview
                var preview = StripEscPosCommands(receipt);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId = order.Id,
                        preview = preview,
                        paperWidth = paperWidth
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing thermal receipt for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Batch generate receipts for multiple orders (e.g., for a trip)
        /// </summary>
        [HttpPost("thermal/batch")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> GenerateBatchReceipts([FromBody] List<long> orderIds, [FromQuery] int paperWidth = 58)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Include(o => o.Payments)
                    .Where(o => orderIds.Contains(o.Id))
                    .AsNoTracking()
                    .ToListAsync();

                if (!orders.Any())
                {
                    return NotFound(new { success = false, message = "No orders found" });
                }

                var receipts = new List<object>();

                foreach (var order in orders)
                {
                    var base64Receipt = _receiptService.GenerateBase64Receipt(order, paperWidth);

                    receipts.Add(new
                    {
                        orderId = order.Id,
                        storeName = order.Store.Name,
                        receiptData = base64Receipt
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = receipts,
                    count = receipts.Count,
                    message = $"{receipts.Count} thermal receipts generated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating batch thermal receipts");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Test printer connection by sending test receipt
        /// </summary>
        [HttpGet("thermal/test")]
        public IActionResult GenerateTestReceipt([FromQuery] int paperWidth = 58)
        {
            try
            {
                var testReceipt = GenerateTestReceiptContent(paperWidth);
                var base64Receipt = Convert.ToBase64String(
                    System.Text.Encoding.GetEncoding("Windows-1252").GetBytes(testReceipt)
                );

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        receiptData = base64Receipt,
                        paperWidth = paperWidth,
                        message = "Test receipt generated"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test receipt");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        #region Helper Methods

        private string StripEscPosCommands(string receipt)
        {
            // Remove ESC/POS control characters for text preview
            var cleaned = receipt;

            // Common ESC/POS sequences to remove
            var escSequences = new[]
            {
                "\x1B@", "\x1BE\x01", "\x1BE\x00", "\x1Ba\x00", "\x1Ba\x01", "\x1Ba\x02",
                "\x1D!\x00", "\x1D!\x11", "\x1D!\x22", "\x1DV\x42\x00", "\x1Bd\x03"
            };

            foreach (var seq in escSequences)
            {
                cleaned = cleaned.Replace(seq, "");
            }

            return cleaned;
        }

        private string GenerateTestReceiptContent(int paperWidth)
        {
            const string ESC = "\x1B";
            const string GS = "\x1D";
            const string INIT = ESC + "@";
            const string ALIGN_CENTER = ESC + "a" + "\x01";
            const string SIZE_DOUBLE = GS + "!" + "\x11";
            const string SIZE_NORMAL = GS + "!" + "\x00";
            const string CUT_PAPER = GS + "V" + "\x42" + "\x00";

            var receipt = new System.Text.StringBuilder();

            receipt.Append(INIT);
            receipt.Append(ALIGN_CENTER);
            receipt.Append(SIZE_DOUBLE);
            receipt.Append("PRINTER TEST\n");
            receipt.Append(SIZE_NORMAL);
            receipt.Append("------------------------\n");
            receipt.Append($"Paper Width: {paperWidth}mm\n");
            receipt.Append($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
            receipt.Append("------------------------\n");
            receipt.Append("If you can read this,\n");
            receipt.Append("your printer is working!\n");
            receipt.Append("\n\n\n");
            receipt.Append(CUT_PAPER);

            return receipt.ToString();
        }

        #endregion
    }
}
