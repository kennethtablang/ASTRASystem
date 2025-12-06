using Microsoft.EntityFrameworkCore;
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Order;
using ASTRASystem.Enum;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ASTRASystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;
        private readonly IPdfService _pdfService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            INotificationService notificationService,
            IPdfService pdfService,
            UserManager<ApplicationUser> userManager,
            ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
            _pdfService = pdfService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(long id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Distributor)
                    .Include(o => o.Warehouse)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                var orderDto = _mapper.Map<OrderDto>(order);

                // Get agent name
                if (!string.IsNullOrEmpty(order.AgentId))
                {
                    var agent = await _userManager.FindByIdAsync(order.AgentId);
                    orderDto.AgentName = agent?.FullName;
                }

                return ApiResponse<OrderDto>.SuccessResponse(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID {Id}", id);
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred while retrieving order");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<OrderListItemDto>>> GetOrdersAsync(OrderQueryDto query)
        {
            try
            {
                var ordersQuery = _context.Orders
                    .Include(o => o.Store)
                    .AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchLower = query.SearchTerm.ToLower();
                    ordersQuery = ordersQuery.Where(o =>
                        o.Store.Name.ToLower().Contains(searchLower) ||
                        o.Id.ToString().Contains(searchLower));
                }

                if (query.Status.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);
                }

                if (query.StoreId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.StoreId == query.StoreId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.AgentId))
                {
                    ordersQuery = ordersQuery.Where(o => o.AgentId == query.AgentId);
                }

                if (query.DistributorId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DistributorId == query.DistributorId.Value);
                }

                if (query.WarehouseId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.WarehouseId == query.WarehouseId.Value);
                }

                if (query.Priority.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.Priority == query.Priority.Value);
                }

                if (query.ScheduledFrom.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.ScheduledFor >= query.ScheduledFrom.Value);
                }

                if (query.ScheduledTo.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.ScheduledFor <= query.ScheduledTo.Value);
                }

                if (query.CreatedFrom.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt >= query.CreatedFrom.Value);
                }

                if (query.CreatedTo.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CreatedAt <= query.CreatedTo.Value);
                }

                // Apply sorting
                ordersQuery = query.SortBy.ToLower() switch
                {
                    "createdat" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.CreatedAt)
                        : ordersQuery.OrderBy(o => o.CreatedAt),
                    "scheduledfor" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.ScheduledFor)
                        : ordersQuery.OrderBy(o => o.ScheduledFor),
                    "total" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.Total)
                        : ordersQuery.OrderBy(o => o.Total),
                    "status" => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.Status)
                        : ordersQuery.OrderBy(o => o.Status),
                    _ => query.SortDescending
                        ? ordersQuery.OrderByDescending(o => o.CreatedAt)
                        : ordersQuery.OrderBy(o => o.CreatedAt)
                };

                var totalCount = await ordersQuery.CountAsync();
                var orders = await ordersQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var orderDtos = new List<OrderListItemDto>();
                foreach (var order in orders)
                {
                    var dto = _mapper.Map<OrderListItemDto>(order);

                    if (!string.IsNullOrEmpty(order.AgentId))
                    {
                        var agent = await _userManager.FindByIdAsync(order.AgentId);
                        dto.AgentName = agent?.FullName;
                    }

                    orderDtos.Add(dto);
                }

                var paginatedResponse = new PaginatedResponse<OrderListItemDto>(
                    orderDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<OrderListItemDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders");
                return ApiResponse<PaginatedResponse<OrderListItemDto>>.ErrorResponse("An error occurred while retrieving orders");
            }
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto request, string agentId)
        {
            try
            {
                // Validate store exists
                var store = await _context.Stores.FindAsync(request.StoreId);
                if (store == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Store not found");
                }

                // Validate products and get their current prices
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != productIds.Distinct().Count())
                {
                    return ApiResponse<OrderDto>.ErrorResponse("One or more products not found");
                }

                // Create order
                var order = new Order
                {
                    StoreId = request.StoreId,
                    AgentId = agentId,
                    DistributorId = request.DistributorId,
                    WarehouseId = request.WarehouseId,
                    Status = OrderStatus.Pending,
                    Priority = request.Priority,
                    ScheduledFor = request.ScheduledFor,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = agentId,
                    UpdatedById = agentId
                };

                // Create order items and calculate totals
                decimal subTotal = 0;

                foreach (var itemDto in request.Items)
                {
                    var product = products.First(p => p.Id == itemDto.ProductId);
                    var unitPrice = itemDto.UnitPrice ?? product.Price;

                    var orderItem = new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = unitPrice,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = agentId,
                        UpdatedById = agentId
                    };

                    order.Items.Add(orderItem);
                    subTotal += orderItem.Quantity * orderItem.UnitPrice;
                }

                // Calculate tax (12% VAT)
                order.SubTotal = subTotal;
                order.Tax = subTotal * 0.12m;
                order.Total = order.SubTotal + order.Tax;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    agentId,
                    "Order created",
                    new { OrderId = order.Id, StoreId = order.StoreId, Total = order.Total });

                // Notify distributor admin and warehouse staff
                if (request.DistributorId.HasValue)
                {
                    await _notificationService.SendNotificationToRoleAsync(
                        "DistributorAdmin",
                        "NewOrder",
                        $"New order #{order.Id} created for {store.Name}");
                }

                // Reload with full details
                order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Distributor)
                    .Include(o => o.Warehouse)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstAsync(o => o.Id == order.Id);

                var orderDto = _mapper.Map<OrderDto>(order);
                var agent = await _userManager.FindByIdAsync(agentId);
                orderDto.AgentName = agent?.FullName;

                return ApiResponse<OrderDto>.SuccessResponse(
                    orderDto,
                    "Order created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred while creating order");
            }
        }

        public async Task<ApiResponse<List<OrderDto>>> BatchCreateOrdersAsync(BatchCreateOrderDto request, string agentId)
        {
            try
            {
                var createdOrders = new List<OrderDto>();
                var errors = new List<string>();

                foreach (var orderDto in request.Orders)
                {
                    var result = await CreateOrderAsync(orderDto, agentId);
                    if (result.Success)
                    {
                        createdOrders.Add(result.Data);
                    }
                    else
                    {
                        errors.Add($"Order for store {orderDto.StoreId}: {result.Message}");
                    }
                }

                if (errors.Any())
                {
                    return ApiResponse<List<OrderDto>>.ErrorResponse(
                        $"Batch completed with {errors.Count} error(s)",
                        errors);
                }

                return ApiResponse<List<OrderDto>>.SuccessResponse(
                    createdOrders,
                    $"{createdOrders.Count} orders created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch order creation");
                return ApiResponse<List<OrderDto>>.ErrorResponse("An error occurred during batch creation");
            }
        }

        public async Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(UpdateOrderStatusDto request, string userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                var oldStatus = order.Status;
                order.Status = request.NewStatus;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order status updated",
                    new
                    {
                        OrderId = order.Id,
                        OldStatus = oldStatus.ToString(),
                        NewStatus = request.NewStatus.ToString(),
                        Notes = request.Notes
                    });

                // Notify relevant parties
                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderStatusChanged",
                    $"Order #{order.Id} status changed to {request.NewStatus}");

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred while updating order status");
            }
        }

        public async Task<ApiResponse<OrderDto>> ConfirmOrderAsync(ConfirmOrderDto request, string userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                if (order.Status != OrderStatus.Pending)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only pending orders can be confirmed");
                }

                // Apply item adjustments if provided
                if (request.ItemAdjustments != null && request.ItemAdjustments.Any())
                {
                    foreach (var adjustment in request.ItemAdjustments)
                    {
                        var item = order.Items.FirstOrDefault(i => i.Id == adjustment.OrderItemId);
                        if (item != null)
                        {
                            item.Quantity = adjustment.NewQuantity;
                            item.UpdatedAt = DateTime.UtcNow;
                            item.UpdatedById = userId;
                        }
                    }

                    // Recalculate totals
                    order.SubTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice);
                    order.Tax = order.SubTotal * 0.12m;
                    order.Total = order.SubTotal + order.Tax;
                }

                order.WarehouseId = request.WarehouseId;
                order.Status = OrderStatus.Confirmed;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order confirmed",
                    new { OrderId = order.Id, WarehouseId = request.WarehouseId, Notes = request.Notes });

                // Notify agent
                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderConfirmed",
                    $"Order #{order.Id} has been confirmed");

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred while confirming order");
            }
        }

        public async Task<ApiResponse<OrderDto>> MarkOrderAsPackedAsync(MarkOrderPackedDto request, string userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                if (order.Status != OrderStatus.Confirmed)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only confirmed orders can be marked as packed");
                }

                order.Status = OrderStatus.Packed;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order marked as packed",
                    new { OrderId = order.Id, Notes = request.Notes });

                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderPacked",
                    $"Order #{order.Id} has been packed and is ready for dispatch");

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as packed");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(long orderId, string userId, string? reason)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Order not found");
                }

                if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot cancel delivered or already cancelled orders");
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order cancelled",
                    new { OrderId = order.Id, Reason = reason });

                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderCancelled",
                    $"Order #{order.Id} has been cancelled");

                return ApiResponse<bool>.SuccessResponse(true, "Order cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<OrderSummaryDto>> GetOrderSummaryAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var query = _context.Orders.AsNoTracking();

                if (from.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= from.Value);
                }

                if (to.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= to.Value);
                }

                var summary = new OrderSummaryDto
                {
                    TotalOrders = await query.CountAsync(),
                    PendingOrders = await query.CountAsync(o => o.Status == OrderStatus.Pending),
                    ConfirmedOrders = await query.CountAsync(o => o.Status == OrderStatus.Confirmed),
                    PackedOrders = await query.CountAsync(o => o.Status == OrderStatus.Packed),
                    DispatchedOrders = await query.CountAsync(o => o.Status == OrderStatus.Dispatched),
                    DeliveredOrders = await query.CountAsync(o => o.Status == OrderStatus.Delivered),
                    TotalValue = await query.SumAsync(o => o.Total)
                };

                summary.AverageOrderValue = summary.TotalOrders > 0
                    ? summary.TotalValue / summary.TotalOrders
                    : 0;

                return ApiResponse<OrderSummaryDto>.SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order summary");
                return ApiResponse<OrderSummaryDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<OrderDto>>> GetOrdersByStatusAsync(OrderStatus status)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.Status == status)
                    .AsNoTracking()
                    .ToListAsync();

                var orderDtos = new List<OrderDto>();
                foreach (var order in orders)
                {
                    var dto = _mapper.Map<OrderDto>(order);

                    if (!string.IsNullOrEmpty(order.AgentId))
                    {
                        var agent = await _userManager.FindByIdAsync(order.AgentId);
                        dto.AgentName = agent?.FullName;
                    }

                    orderDtos.Add(dto);
                }

                return ApiResponse<List<OrderDto>>.SuccessResponse(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status");
                return ApiResponse<List<OrderDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<OrderDto>>> GetOrdersReadyForDispatchAsync(long? warehouseId = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => o.Status == OrderStatus.Packed)
                    .AsNoTracking();

                if (warehouseId.HasValue)
                {
                    query = query.Where(o => o.WarehouseId == warehouseId.Value);
                }

                var orders = await query.OrderBy(o => o.Priority ? 0 : 1)
                    .ThenBy(o => o.CreatedAt)
                    .ToListAsync();

                var orderDtos = new List<OrderDto>();
                foreach (var order in orders)
                {
                    var dto = _mapper.Map<OrderDto>(order);

                    if (!string.IsNullOrEmpty(order.AgentId))
                    {
                        var agent = await _userManager.FindByIdAsync(order.AgentId);
                        dto.AgentName = agent?.FullName;
                    }

                    orderDtos.Add(dto);
                }

                return ApiResponse<List<OrderDto>>.SuccessResponse(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders ready for dispatch");
                return ApiResponse<List<OrderDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<byte[]>> GeneratePickListAsync(long warehouseId, List<long> orderIds)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .Where(o => orderIds.Contains(o.Id) && o.WarehouseId == warehouseId)
                    .ToListAsync();

                if (!orders.Any())
                {
                    return ApiResponse<byte[]>.ErrorResponse("No orders found");
                }

                var pdfBytes = _pdfService.GeneratePickListPdf(orders);
                return ApiResponse<byte[]>.SuccessResponse(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pick list");
                return ApiResponse<byte[]>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<byte[]>> GeneratePackingSlipAsync(long orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse<byte[]>.ErrorResponse("Order not found");
                }

                var pdfBytes = _pdfService.GeneratePackingSlipPdf(order);
                return ApiResponse<byte[]>.SuccessResponse(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating packing slip");
                return ApiResponse<byte[]>.ErrorResponse("An error occurred");
            }
        }
        public async Task<ApiResponse<OrderDto>> DispatchOrderAsync(long orderId, string userId, long tripId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                if (order.Status != OrderStatus.Packed)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only packed orders can be dispatched");
                }

                // Verify trip exists
                var trip = await _context.Trips.FindAsync(tripId);
                if (trip == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Trip not found");
                }

                order.Status = OrderStatus.Dispatched;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order dispatched",
                    new { OrderId = order.Id, TripId = tripId });

                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderDispatched",
                    $"Order #{order.Id} has been dispatched");

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching order");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred while dispatching order");
            }
        }

        public async Task<ApiResponse<OrderDto>> MarkOrderInTransitAsync(long orderId, string userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                if (order.Status != OrderStatus.Dispatched)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only dispatched orders can be marked as in transit");
                }

                order.Status = OrderStatus.InTransit;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order marked in transit",
                    new { OrderId = order.Id });

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order in transit");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<OrderDto>> MarkOrderAtStoreAsync(long orderId, string userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                if (order.Status != OrderStatus.InTransit)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only in-transit orders can be marked as at store");
                }

                order.Status = OrderStatus.AtStore;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order marked at store",
                    new { OrderId = order.Id });

                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderAtStore",
                    $"Order #{order.Id} has arrived at the store");

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order at store");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<OrderDto>> MarkOrderDeliveredAsync(long orderId, string userId, string? notes)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                if (order.Status != OrderStatus.AtStore)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only orders at store can be marked as delivered");
                }

                order.Status = OrderStatus.Delivered;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order delivered",
                    new { OrderId = order.Id, Notes = notes });

                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderDelivered",
                    $"Order #{order.Id} has been successfully delivered");

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as delivered");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<OrderDto>> MarkOrderReturnedAsync(long orderId, string userId, string reason)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                // Allow return from InTransit, AtStore, or Delivered status
                if (order.Status != OrderStatus.InTransit &&
                    order.Status != OrderStatus.AtStore &&
                    order.Status != OrderStatus.Delivered)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only in-transit, at-store, or delivered orders can be marked as returned");
                }

                order.Status = OrderStatus.Returned;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order returned",
                    new { OrderId = order.Id, Reason = reason });

                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderReturned",
                    $"Order #{order.Id} has been returned. Reason: {reason}");

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as returned");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<OrderDto>> EditOrderAsync(long orderId, UpdateOrderDto request, string userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse<OrderDto>.ErrorResponse("Order not found");
                }

                // Only allow editing of Pending orders
                if (order.Status != OrderStatus.Pending)
                {
                    return ApiResponse<OrderDto>.ErrorResponse(
                        "Only pending orders can be edited");
                }

                // Update order properties
                order.Priority = request.Priority;
                order.ScheduledFor = request.ScheduledFor;
                order.DistributorId = request.DistributorId;
                order.WarehouseId = request.WarehouseId;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                // Remove existing items
                _context.OrderItems.RemoveRange(order.Items);

                // Add new items
                var productIds = request.Items.Select(i => i.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                decimal subTotal = 0;

                foreach (var itemDto in request.Items)
                {
                    var product = products.First(p => p.Id == itemDto.ProductId);
                    var unitPrice = itemDto.UnitPrice ?? product.Price;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = unitPrice,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = userId,
                        UpdatedById = userId
                    };

                    order.Items.Add(orderItem);
                    subTotal += orderItem.Quantity * orderItem.UnitPrice;
                }

                // Recalculate totals
                order.SubTotal = subTotal;
                order.Tax = subTotal * 0.12m;
                order.Total = order.SubTotal + order.Tax;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order edited",
                    new { OrderId = order.Id });

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing order");
                return ApiResponse<OrderDto>.ErrorResponse("An error occurred while editing order");
            }
        }
    }
}
