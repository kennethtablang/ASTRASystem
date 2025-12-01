using Microsoft.EntityFrameworkCore;
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ASTRASystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string userId, string type, string payload)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Type = type,
                    Payload = payload,
                    IsRead = false,
                    CreatedById = "system",
                    UpdatedById = "system"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // In production, also send via SignalR, push notifications, etc.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToRoleAsync(string role, string type, string payload)
        {
            try
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var notifications = usersInRole.Select(user => new Notification
                {
                    UserId = user.Id,
                    Type = type,
                    Payload = payload,
                    IsRead = false,
                    CreatedById = "system",
                    UpdatedById = "system"
                }).ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to role {Role}", role);
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
        {
            try
            {
                var query = _context.Notifications.Where(n => n.UserId == userId);

                if (unreadOnly)
                {
                    query = query.Where(n => !n.IsRead);
                }

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);

                return ApiResponse<List<NotificationDto>>.SuccessResponse(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return ApiResponse<List<NotificationDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(long notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Notification not found");
                }

                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Notification marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "All notifications marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
        {
            try
            {
                var count = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .CountAsync();

                return ApiResponse<int>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return ApiResponse<int>.ErrorResponse("An error occurred");
            }
        }
    }
}
