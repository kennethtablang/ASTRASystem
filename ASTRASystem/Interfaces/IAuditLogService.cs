using ASTRASystem.DTO.Common;

namespace ASTRASystem.Interfaces
{
    public interface IAuditLogService
    {
        Task LogActionAsync(string userId, string action, object? metadata = null);
        Task<ApiResponse<PaginatedResponse<AuditLogDto>>> GetAuditLogsAsync(AuditLogQueryDto query);
        Task<ApiResponse<List<AuditLogDto>>> GetUserAuditLogsAsync(string userId, int limit = 50);
    }
}
