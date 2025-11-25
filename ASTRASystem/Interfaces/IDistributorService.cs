using ASTRASystem.DTO.Common;

namespace ASTRASystem.Interfaces
{
    public interface IDistributorService
    {
        Task<ApiResponse<DistributorDto>> GetDistributorByIdAsync(long id);
        Task<ApiResponse<List<DistributorDto>>> GetDistributorsAsync();
        Task<ApiResponse<DistributorDto>> CreateDistributorAsync(DistributorDto request, string userId);
        Task<ApiResponse<DistributorDto>> UpdateDistributorAsync(DistributorDto request, string userId);
        Task<ApiResponse<bool>> DeleteDistributorAsync(long id);
    }
}
