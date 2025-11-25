using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Order;
using ASTRASystem.Enum;

namespace ASTRASystem.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(long id);
        Task<ApiResponse<PaginatedResponse<OrderListItemDto>>> GetOrdersAsync(OrderQueryDto query);
        Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderDto request, string agentId);
        Task<ApiResponse<List<OrderDto>>> BatchCreateOrdersAsync(BatchCreateOrderDto request, string agentId);
        Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(UpdateOrderStatusDto request, string userId);
        Task<ApiResponse<OrderDto>> ConfirmOrderAsync(ConfirmOrderDto request, string userId);
        Task<ApiResponse<OrderDto>> MarkOrderAsPackedAsync(MarkOrderPackedDto request, string userId);
        Task<ApiResponse<bool>> CancelOrderAsync(long orderId, string userId, string? reason);
        Task<ApiResponse<OrderSummaryDto>> GetOrderSummaryAsync(DateTime? from = null, DateTime? to = null);
        Task<ApiResponse<List<OrderDto>>> GetOrdersByStatusAsync(OrderStatus status);
        Task<ApiResponse<List<OrderDto>>> GetOrdersReadyForDispatchAsync(long? warehouseId = null);
        Task<ApiResponse<byte[]>> GeneratePickListAsync(long warehouseId, List<long> orderIds);
        Task<ApiResponse<byte[]>> GeneratePackingSlipAsync(long orderId);
    }
}
