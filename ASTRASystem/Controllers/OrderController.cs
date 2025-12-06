using ASTRASystem.DTO.Order;
using ASTRASystem.Enum;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            // Use ClaimTypes.NameIdentifier for the user ID - SAME AS PRODUCT/INVENTORY CONTROLLERS
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateOrder: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CreateOrder: User {UserId} creating order for store {StoreId}", userId, request.StoreId);

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("BatchCreateOrders: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("BatchCreateOrders: User {UserId} creating {Count} orders", userId, request.Orders.Count);

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateOrderStatus: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateOrderStatus: User {UserId} updating order {OrderId}", userId, request.OrderId);

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("ConfirmOrder: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("ConfirmOrder: User {UserId} confirming order {OrderId}", userId, request.OrderId);

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkOrderAsPacked: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkOrderAsPacked: User {UserId} marking order {OrderId} as packed", userId, request.OrderId);

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CancelOrder: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("CancelOrder: User {UserId} cancelling order {OrderId}", userId, id);

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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Agent")]
        public async Task<IActionResult> EditOrder(long id, [FromBody] UpdateOrderDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("EditOrder: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            if (id != request.OrderId)
            {
                return BadRequest(new { success = false, message = "Order ID mismatch" });
            }

            _logger.LogInformation("EditOrder: User {UserId} editing order {OrderId}", userId, id);

            var result = await _orderService.EditOrderAsync(id, request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/dispatch")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> DispatchOrder(long id, [FromBody] DispatchOrderDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("DispatchOrder: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("DispatchOrder: User {UserId} dispatching order {OrderId}", userId, id);

            var result = await _orderService.DispatchOrderAsync(id, userId, request.TripId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/in-transit")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> MarkOrderInTransit(long id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkOrderInTransit: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkOrderInTransit: User {UserId} marking order {OrderId} in transit", userId, id);

            var result = await _orderService.MarkOrderInTransitAsync(id, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/at-store")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> MarkOrderAtStore(long id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkOrderAtStore: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkOrderAtStore: User {UserId} marking order {OrderId} at store", userId, id);

            var result = await _orderService.MarkOrderAtStoreAsync(id, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/delivered")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> MarkOrderDelivered(long id, [FromBody] MarkOrderDeliveredDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkOrderDelivered: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkOrderDelivered: User {UserId} marking order {OrderId} delivered", userId, id);

            var result = await _orderService.MarkOrderDeliveredAsync(id, userId, request.Notes);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/returned")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> MarkOrderReturned(long id, [FromBody] MarkOrderReturnedDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkOrderReturned: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkOrderReturned: User {UserId} marking order {OrderId} returned", userId, id);

            var result = await _orderService.MarkOrderReturnedAsync(id, userId, request.Reason);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}