using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Location;

namespace ASTRASystem.Interfaces
{
    public interface ICityService
    {
        Task<ApiResponse<CityDto>> GetCityByIdAsync(long id);
        Task<ApiResponse<CityDto>> GetCityByNameAsync(string name);
        Task<ApiResponse<PaginatedResponse<CityDto>>> GetCitiesAsync(CityQueryDto query);
        Task<ApiResponse<List<CityListItemDto>>> GetCitiesForLookupAsync(string? searchTerm = null);
        Task<ApiResponse<CityDto>> CreateCityAsync(CreateCityDto request, string userId);
        Task<ApiResponse<CityDto>> UpdateCityAsync(UpdateCityDto request, string userId);
        Task<ApiResponse<bool>> DeleteCityAsync(long id);
        Task<ApiResponse<List<string>>> GetProvincesAsync();
        Task<ApiResponse<List<string>>> GetRegionsAsync();
    }
}
