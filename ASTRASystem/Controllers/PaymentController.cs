using ASTRASystem.DTO.Payment;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            IInvoiceService invoiceService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant,Dispatcher")]
        public async Task<IActionResult> GetPaymentById(long id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GetPayments([FromQuery] PaymentQueryDto query)
        {
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    query.DistributorId = userDistributorId;
                }
            }

            var result = await _paymentService.GetPaymentsAsync(query);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant,Dispatcher")]
        public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("RecordPayment: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("RecordPayment: User {UserId} recording payment for order {OrderId}", userId, request.OrderId);

            var result = await _paymentService.RecordPaymentAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant,Agent,Dispatcher")]
        public async Task<IActionResult> GetPaymentsByOrder(long orderId)
        {
            var result = await _paymentService.GetPaymentsByOrderAsync(orderId);
            return Ok(result);
        }

        [HttpPost("reconcile")]
        [Authorize(Roles = "Admin,Accountant,DistributorAdmin")]
        public async Task<IActionResult> ReconcilePayment([FromBody] ReconcilePaymentDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("ReconcilePayment: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("ReconcilePayment: User {UserId} reconciling payment {PaymentId}", userId, request.PaymentId);

            long? distributorId = null;
            if (User.IsInRole("DistributorAdmin"))
            {
                var claimDistributorId = User.FindFirst("DistributorId")?.Value;
                if (long.TryParse(claimDistributorId, out long userDistributorId))
                {
                    distributorId = userDistributorId;
                }
            }

            var result = await _paymentService.ReconcilePaymentAsync(request, userId, distributorId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("cash-collection")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant,Dispatcher")]
        public async Task<IActionResult> GetCashCollectionSummary(
            [FromQuery] long? tripId = null,
            [FromQuery] string? dispatcherId = null,
            [FromQuery] DateTime? date = null)
        {
            var result = await _paymentService.GetCashCollectionSummaryAsync(tripId, dispatcherId, date);
            return Ok(result);
        }

        [HttpGet("unreconciled")]
        [Authorize(Roles = "Admin,Accountant,DistributorAdmin")]
        public async Task<IActionResult> GetUnreconciledPayments()
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

            var result = await _paymentService.GetUnreconciledPaymentsAsync(distributorId);
            return Ok(result);
        }

        [HttpGet("order/{orderId}/balance")]
        public async Task<IActionResult> GetOrderBalance(long orderId)
        {
            var result = await _paymentService.GetOrderBalanceAsync(orderId);
            return Ok(result);
        }

        // Invoice endpoints
        [HttpGet("invoice/{id}")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GetInvoiceById(long id)
        {
            var result = await _invoiceService.GetInvoiceByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("invoice/order/{orderId}")]
        public async Task<IActionResult> GetInvoiceByOrderId(long orderId)
        {
            var result = await _invoiceService.GetInvoiceByOrderIdAsync(orderId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost("invoice")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GenerateInvoice: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("GenerateInvoice: User {UserId} generating invoice for order {OrderId}", userId, request.OrderId);

            var result = await _invoiceService.GenerateInvoiceAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("invoice/{id}/pdf")]
        public async Task<IActionResult> GenerateInvoicePdf(long id)
        {
            var result = await _invoiceService.GenerateInvoicePdfAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return File(result.Data, "application/pdf", $"invoice_{id}.pdf");
        }

        [HttpGet("ar/summary")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GetARSummary()
        {
            var result = await _invoiceService.GetARSummaryAsync();
            return Ok(result);
        }

        [HttpGet("ar/aging")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GetARAgingReport()
        {
            var result = await _invoiceService.GetARAgingReportAsync();
            return Ok(result);
        }

        [HttpGet("invoice/overdue")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GetOverdueInvoices()
        {
            var result = await _invoiceService.GetOverdueInvoicesAsync();
            return Ok(result);
        }

        [HttpGet("invoice/store/{storeId}")]
        public async Task<IActionResult> GetInvoicesByStore(long storeId)
        {
            var result = await _invoiceService.GetInvoicesByStoreAsync(storeId);
            return Ok(result);
        }
    }
}
