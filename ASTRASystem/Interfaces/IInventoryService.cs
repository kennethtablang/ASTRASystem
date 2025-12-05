using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Inventory;

namespace ASTRASystem.Interfaces
{
    public interface IInventoryService
    {
        Task<ApiResponse<InventoryDto>> GetInventoryByIdAsync(long id);
        Task<ApiResponse<PaginatedResponse<InventoryDto>>> GetInventoriesAsync(InventoryQueryDto query);
        Task<ApiResponse<InventoryDto>> CreateInventoryAsync(CreateInventoryDto request, string userId);
        Task<ApiResponse<InventoryDto>> AdjustInventoryAsync(AdjustInventoryDto request, string userId);
        Task<ApiResponse<InventoryDto>> RestockInventoryAsync(RestockInventoryDto request, string userId);
        Task<ApiResponse<InventoryDto>> UpdateInventoryLevelsAsync(UpdateInventoryLevelsDto request, string userId);
        Task<ApiResponse<List<InventoryMovementDto>>> GetInventoryMovementsAsync(long inventoryId, int limit = 50);
        Task<ApiResponse<InventorySummaryDto>> GetInventorySummaryAsync(long? warehouseId = null);
        Task<ApiResponse<bool>> DeleteInventoryAsync(long id);
    }
}
