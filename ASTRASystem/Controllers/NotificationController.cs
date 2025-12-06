using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            IAuditLogService auditLogService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications([FromQuery] bool unreadOnly = false)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetUserNotifications: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            var result = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            return Ok(result);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkAsRead: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkAsRead: User {UserId} marking notification {NotificationId} as read", userId, id);

            var result = await _notificationService.MarkAsReadAsync(id, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkAllAsRead: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkAllAsRead: User {UserId} marking all notifications as read", userId);

            var result = await _notificationService.MarkAllAsReadAsync(userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetUnreadCount: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            var result = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(result);
        }

        // Audit Log endpoints
        [HttpGet("audit-logs")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQueryDto query)
        {
            var result = await _auditLogService.GetAuditLogsAsync(query);
            return Ok(result);
        }

        [HttpGet("audit-logs/user/{userId}")]
        [Authorize(Roles = "Admin,DistributorAdmin")]
        public async Task<IActionResult> GetUserAuditLogs(string userId, [FromQuery] int limit = 50)
        {
            var result = await _auditLogService.GetUserAuditLogsAsync(userId, limit);
            return Ok(result);
        }

        [HttpGet("audit-logs/me")]
        public async Task<IActionResult> GetMyAuditLogs([FromQuery] int limit = 50)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetMyAuditLogs: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            var result = await _auditLogService.GetUserAuditLogsAsync(userId, limit);
            return Ok(result);
        }
    }
}
