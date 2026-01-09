using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.User;

namespace ASTRASystem.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId);
        Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email);
        Task<ApiResponse<PaginatedResponse<UserListItemDto>>> GetUsersAsync(UserQueryDto query);
        Task<ApiResponse<UserDto>> UpdateUserProfileAsync(string userId, UpdateUserProfileDto request);
        Task<ApiResponse<bool>> ApproveUserAsync(ApproveUserDto request);
        Task<ApiResponse<bool>> AssignRolesAsync(AssignRolesDto request);
        Task<ApiResponse<List<string>>> GetUserRolesAsync(string userId);
        Task<ApiResponse<bool>> DeleteUserAsync(string userId);
        Task<ApiResponse<List<string>>> GetAllRolesAsync();
        Task<ApiResponse<List<UserListItemDto>>> GetUsersByRoleAsync(string role, long? distributorId = null);
        Task<ApiResponse<List<UserListItemDto>>> GetUnapprovedUsersAsync();
    }
}
