using ASTRASystem.DTO.Delivery;
using ASTRASystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASTRASystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        private readonly ILogger<DeliveryController> _logger;

        public DeliveryController(IDeliveryService deliveryService, ILogger<DeliveryController> logger)
        {
            _deliveryService = deliveryService;
            _logger = logger;
        }

        [HttpPost("photo")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> UploadDeliveryPhoto([FromForm] UploadDeliveryPhotoDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UploadDeliveryPhoto: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UploadDeliveryPhoto: User {UserId} uploading photo for order {OrderId}", userId, request.OrderId);

            var result = await _deliveryService.UploadDeliveryPhotoAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("order/{orderId}/photos")]
        public async Task<IActionResult> GetDeliveryPhotos(long orderId)
        {
            var result = await _deliveryService.GetDeliveryPhotosAsync(orderId);
            return Ok(result);
        }

        [HttpPost("location")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UpdateLocation: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("UpdateLocation: User {UserId} updating location for trip {TripId}", userId, request.TripId);

            var result = await _deliveryService.UpdateLocationAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("trip/{tripId}/tracking")]
        public async Task<IActionResult> GetLiveTripTracking(long tripId)
        {
            var result = await _deliveryService.GetLiveTripTrackingAsync(tripId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost("mark-delivered")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> MarkOrderAsDelivered([FromForm] MarkDeliveredDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("MarkOrderAsDelivered: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("MarkOrderAsDelivered: User {UserId} marking order {OrderId} as delivered", userId, request.OrderId);

            var result = await _deliveryService.MarkOrderAsDeliveredAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("exception")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> ReportDeliveryException([FromForm] ReportDeliveryExceptionDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("ReportDeliveryException: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("ReportDeliveryException: User {UserId} reporting exception for order {OrderId}", userId, request.OrderId);

            var result = await _deliveryService.ReportDeliveryExceptionAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("attempt")]
        [Authorize(Roles = "Admin,DistributorAdmin,Dispatcher")]
        public async Task<IActionResult> RecordDeliveryAttempt([FromBody] DeliveryAttemptDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("RecordDeliveryAttempt: User ID not found in claims");
                return Unauthorized(new { success = false, message = "User authentication failed" });
            }

            _logger.LogInformation("RecordDeliveryAttempt: User {UserId} recording attempt for order {OrderId}", userId, request.OrderId);

            var result = await _deliveryService.RecordDeliveryAttemptAsync(request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("exceptions")]
        [Authorize(Roles = "Admin,DistributorAdmin,Agent")]
        public async Task<IActionResult> GetDeliveryExceptions([FromQuery] long? orderId = null)
        {
            var result = await _deliveryService.GetDeliveryExceptionsAsync(orderId);
            return Ok(result);
        }
        [HttpGet("trip/{tripId}/history")]
        public async Task<IActionResult> GetTripLocationHistory(long tripId)
        {
            var result = await _deliveryService.GetTripLocationHistoryAsync(tripId);
            return Ok(result);
        }
    }
}
