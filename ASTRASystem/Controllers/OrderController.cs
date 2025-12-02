using ASTRASystem.DTO.Order;
using ASTRASystem.Enum;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(long id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderQueryDto query)
        {
            var result = await _orderService.GetOrdersAsync(query);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,DistributorAdmin,Agent")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.CreateOrderAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Data.Id }, result);
        }

        [HttpPost("batch")]
        [Authorize(Roles = "Admin,DistributorAdmin,Agent")]
        public async Task<IActionResult> BatchCreateOrders([FromBody] BatchCreateOrderDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.BatchCreateOrdersAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.UpdateOrderStatusAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("confirm")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> ConfirmOrder([FromBody] ConfirmOrderDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.ConfirmOrderAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("pack")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> MarkOrderAsPacked([FromBody] MarkOrderPackedDto request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.MarkOrderAsPackedAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,DistributorAdmin,Agent")]
        public async Task<IActionResult> CancelOrder(long id, [FromQuery] string? reason)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _orderService.CancelOrderAsync(id, userId, reason);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Admin,DistributorAdmin,Accountant")]
        public async Task<IActionResult> GetOrderSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _orderService.GetOrderSummaryAsync(from, to);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
        {
            var result = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(result);
        }

        [HttpGet("ready-for-dispatch")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> GetOrdersReadyForDispatch([FromQuery] long? warehouseId = null)
        {
            var result = await _orderService.GetOrdersReadyForDispatchAsync(warehouseId);
            return Ok(result);
        }

        [HttpPost("pick-list")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> GeneratePickList([FromQuery] long warehouseId, [FromBody] List<long> orderIds)
        {
            var result = await _orderService.GeneratePickListAsync(warehouseId, orderIds);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return File(result.Data, "application/pdf", $"picklist_{warehouseId}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet("{id}/packing-slip")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> GeneratePackingSlip(long id)
        {
            var result = await _orderService.GeneratePackingSlipAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return File(result.Data, "application/pdf", $"packing_slip_{id}.pdf");
        }
    }
}
