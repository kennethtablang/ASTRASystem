using ASTRASystem.DTO.Common;

namespace ASTRASystem.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string type, string payload);
        Task SendNotificationToRoleAsync(string role, string type, string payload);
        Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task<ApiResponse<bool>> MarkAsReadAsync(long notificationId, string userId);
        Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId);
        Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
    }
}
