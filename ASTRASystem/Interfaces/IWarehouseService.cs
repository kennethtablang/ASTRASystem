using ASTRASystem.DTO.Common;

namespace ASTRASystem.Interfaces
{
    public interface IWarehouseService
    {
        Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(long id);
        Task<ApiResponse<List<WarehouseDto>>> GetWarehousesAsync(long? distributorId = null);
        Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto request, string userId);
        Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(WarehouseDto request, string userId);
        Task<ApiResponse<bool>> DeleteWarehouseAsync(long id);
    }
}
