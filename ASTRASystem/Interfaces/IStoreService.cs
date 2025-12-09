using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Store;

namespace ASTRASystem.Interfaces
{
    public interface IStoreService
    {
        Task<ApiResponse<StoreDto>> GetStoreByIdAsync(long id);
        Task<ApiResponse<PaginatedResponse<StoreDto>>> GetStoresAsync(StoreQueryDto query);
        Task<ApiResponse<List<StoreListItemDto>>> GetStoresForLookupAsync(string? searchTerm = null);
        Task<ApiResponse<StoreDto>> CreateStoreAsync(CreateStoreDto request, string userId);
        Task<ApiResponse<StoreDto>> UpdateStoreAsync(UpdateStoreDto request, string userId);
        Task<ApiResponse<bool>> DeleteStoreAsync(long id);
        Task<ApiResponse<bool>> UpdateCreditLimitAsync(UpdateCreditLimitDto request, string userId);
        Task<ApiResponse<StoreWithBalanceDto>> GetStoreWithBalanceAsync(long id);
        Task<ApiResponse<List<StoreWithBalanceDto>>> GetStoresWithOutstandingBalanceAsync();
    }
}
