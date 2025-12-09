using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Location;

namespace ASTRASystem.Interfaces
{
    public interface IBarangayService
    {
        Task<ApiResponse<BarangayDto>> GetBarangayByIdAsync(long id);
        Task<ApiResponse<BarangayDto>> GetBarangayByNameAsync(string name, long cityId);
        Task<ApiResponse<PaginatedResponse<BarangayDto>>> GetBarangaysAsync(BarangayQueryDto query);
        Task<ApiResponse<List<BarangayListItemDto>>> GetBarangaysByCityAsync(long cityId);
        Task<ApiResponse<List<BarangayListItemDto>>> GetBarangaysForLookupAsync(long? cityId = null, string? searchTerm = null);
        Task<ApiResponse<BarangayDto>> CreateBarangayAsync(CreateBarangayDto request, string userId);
        Task<ApiResponse<List<BarangayDto>>> BulkCreateBarangaysAsync(BulkCreateBarangaysDto request, string userId);
        Task<ApiResponse<BarangayDto>> UpdateBarangayAsync(UpdateBarangayDto request, string userId);
        Task<ApiResponse<bool>> DeleteBarangayAsync(long id);
    }
}
