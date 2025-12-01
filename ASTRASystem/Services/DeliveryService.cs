using Microsoft.EntityFrameworkCore;
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Delivery;
using ASTRASystem.Enum;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ASTRASystem.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DeliveryService> _logger;

        public DeliveryService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            INotificationService notificationService,
            IFileStorageService fileStorageService,
            UserManager<ApplicationUser> userManager,
            ILogger<DeliveryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApiResponse<DeliveryPhotoDto>> UploadDeliveryPhotoAsync(UploadDeliveryPhotoDto request, string userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return ApiResponse<DeliveryPhotoDto>.ErrorResponse("Order not found");
                }

                // Upload photo to storage
                using var stream = request.Photo.OpenReadStream();
                var fileName = $"delivery_{request.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{request.Photo.FileName}";
                var fileUrl = await _fileStorageService.UploadFileAsync(
                    stream,
                    fileName,
                    request.Photo.ContentType);

                var deliveryPhoto = new DeliveryPhoto
                {
                    OrderId = request.OrderId,
                    Url = fileUrl,
                    UploadedById = userId,
                    UploadedAt = DateTime.UtcNow,
                    Lat = request.Lat,
                    Lng = request.Lng,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                _context.DeliveryPhotos.Add(deliveryPhoto);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Delivery photo uploaded",
                    new { OrderId = request.OrderId, PhotoId = deliveryPhoto.Id });

                var dto = _mapper.Map<DeliveryPhotoDto>(deliveryPhoto);
                var user = await _userManager.FindByIdAsync(userId);
                dto.UploadedByName = user?.FullName;

                return ApiResponse<DeliveryPhotoDto>.SuccessResponse(
                    dto,
                    "Photo uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading delivery photo");
                return ApiResponse<DeliveryPhotoDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<DeliveryPhotoDto>>> GetDeliveryPhotosAsync(long orderId)
        {
            try
            {
                var photos = await _context.DeliveryPhotos
                    .Where(p => p.OrderId == orderId)
                    .AsNoTracking()
                    .OrderBy(p => p.UploadedAt)
                    .ToListAsync();

                var photoDtos = new List<DeliveryPhotoDto>();
                foreach (var photo in photos)
                {
                    var dto = _mapper.Map<DeliveryPhotoDto>(photo);

                    if (!string.IsNullOrEmpty(photo.UploadedById))
                    {
                        var user = await _userManager.FindByIdAsync(photo.UploadedById);
                        dto.UploadedByName = user?.FullName;
                    }

                    photoDtos.Add(dto);
                }

                return ApiResponse<List<DeliveryPhotoDto>>.SuccessResponse(photoDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery photos");
                return ApiResponse<List<DeliveryPhotoDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> UpdateLocationAsync(LocationUpdateDto request, string userId)
        {
            try
            {
                var trip = await _context.Trips.FindAsync(request.TripId);
                if (trip == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Trip not found");
                }

                // In a real system, you'd store location updates in a separate table
                // For now, we'll just log the location update
                await _auditLogService.LogActionAsync(
                    userId,
                    "Location updated",
                    new
                    {
                        TripId = request.TripId,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        Speed = request.Speed,
                        Accuracy = request.Accuracy
                    });

                return ApiResponse<bool>.SuccessResponse(true, "Location updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<LiveTripTrackingDto>> GetLiveTripTrackingAsync(long tripId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Store)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tripId);

                if (trip == null)
                {
                    return ApiResponse<LiveTripTrackingDto>.ErrorResponse("Trip not found");
                }

                var dispatcher = !string.IsNullOrEmpty(trip.DispatcherId)
                    ? await _userManager.FindByIdAsync(trip.DispatcherId)
                    : null;

                var tracking = new LiveTripTrackingDto
                {
                    TripId = trip.Id,
                    DispatcherName = dispatcher?.FullName ?? "Unassigned",
                    Vehicle = trip.Vehicle,
                    TotalStops = trip.Assignments.Count
                };

                // In a real system, you'd retrieve the latest location from a location tracking table
                // For now, we'll leave it null
                tracking.CurrentLatitude = null;
                tracking.CurrentLongitude = null;
                tracking.LastLocationUpdate = null;

                foreach (var assignment in trip.Assignments.OrderBy(a => a.SequenceNo))
                {
                    var order = assignment.Order;
                    var deliveredAt = order.Status == OrderStatus.Delivered
                        ? order.UpdatedAt
                        : null;

                    // Check if order has delivery exception
                    var hasException = await _context.AuditLogs
                        .AnyAsync(al => al.Action == "Delivery exception reported" &&
                                       al.Meta.Contains($"\"OrderId\":{order.Id}"));

                    tracking.Stops.Add(new TripStopStatusDto
                    {
                        OrderId = order.Id,
                        SequenceNo = assignment.SequenceNo,
                        StoreName = order.Store.Name,
                        Status = order.Status.ToString(),
                        DeliveredAt = deliveredAt,
                        HasException = hasException
                    });

                    if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.AtStore)
                    {
                        tracking.CompletedStops++;
                    }
                }

                return ApiResponse<LiveTripTrackingDto>.SuccessResponse(tracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting live trip tracking");
                return ApiResponse<LiveTripTrackingDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> MarkOrderAsDeliveredAsync(MarkDeliveredDto request, string userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Order not found");
                }

                if (order.Status != OrderStatus.InTransit && order.Status != OrderStatus.AtStore)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Only in-transit or at-store orders can be marked as delivered");
                }

                // Upload photos if provided
                if (request.Photos != null && request.Photos.Any())
                {
                    foreach (var photo in request.Photos)
                    {
                        using var stream = photo.OpenReadStream();
                        var fileName = $"delivery_{request.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{photo.FileName}";
                        var fileUrl = await _fileStorageService.UploadFileAsync(
                            stream,
                            fileName,
                            photo.ContentType);

                        var deliveryPhoto = new DeliveryPhoto
                        {
                            OrderId = request.OrderId,
                            Url = fileUrl,
                            UploadedById = userId,
                            UploadedAt = DateTime.UtcNow,
                            Lat = request.Latitude,
                            Lng = request.Longitude,
                            Notes = request.Notes,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = userId,
                            UpdatedById = userId
                        };

                        _context.DeliveryPhotos.Add(deliveryPhoto);
                    }
                }

                order.Status = OrderStatus.Delivered;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Order marked as delivered",
                    new
                    {
                        OrderId = request.OrderId,
                        RecipientName = request.RecipientName,
                        RecipientPhone = request.RecipientPhone,
                        Notes = request.Notes
                    });

                // Notify agent
                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "OrderDelivered",
                    $"Order #{order.Id} for {order.Store.Name} has been delivered");

                return ApiResponse<bool>.SuccessResponse(true, "Order marked as delivered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as delivered");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<DeliveryExceptionDto>> ReportDeliveryExceptionAsync(ReportDeliveryExceptionDto request, string userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return ApiResponse<DeliveryExceptionDto>.ErrorResponse("Order not found");
                }

                // Upload exception photos if provided
                var photoUrls = new List<string>();
                if (request.Photos != null && request.Photos.Any())
                {
                    foreach (var photo in request.Photos)
                    {
                        using var stream = photo.OpenReadStream();
                        var fileName = $"exception_{request.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{photo.FileName}";
                        var fileUrl = await _fileStorageService.UploadFileAsync(
                            stream,
                            fileName,
                            photo.ContentType);

                        photoUrls.Add(fileUrl);
                    }
                }

                await _auditLogService.LogActionAsync(
                    userId,
                    "Delivery exception reported",
                    new
                    {
                        OrderId = request.OrderId,
                        ExceptionType = request.ExceptionType,
                        Description = request.Description,
                        PhotoUrls = photoUrls
                    });

                // Notify relevant parties
                await _notificationService.SendNotificationAsync(
                    order.AgentId,
                    "DeliveryException",
                    $"Delivery exception reported for order #{order.Id}: {request.ExceptionType}");

                await _notificationService.SendNotificationToRoleAsync(
                    "DistributorAdmin",
                    "DeliveryException",
                    $"Delivery exception reported for order #{order.Id}");

                var user = await _userManager.FindByIdAsync(userId);

                var exceptionDto = new DeliveryExceptionDto
                {
                    Id = 0, // This would come from a DeliveryException entity
                    OrderId = request.OrderId,
                    ExceptionType = request.ExceptionType,
                    Description = request.Description,
                    ReportedById = userId,
                    ReportedByName = user?.FullName,
                    ReportedAt = DateTime.UtcNow
                };

                return ApiResponse<DeliveryExceptionDto>.SuccessResponse(
                    exceptionDto,
                    "Exception reported successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting delivery exception");
                return ApiResponse<DeliveryExceptionDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> RecordDeliveryAttemptAsync(DeliveryAttemptDto request, string userId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Order not found");
                }

                await _auditLogService.LogActionAsync(
                    userId,
                    "Delivery attempt recorded",
                    new
                    {
                        OrderId = request.OrderId,
                        Result = request.AttemptResult,
                        Notes = request.Notes,
                        AttemptedAt = request.AttemptedAt
                    });

                // Update order status based on attempt result
                if (request.AttemptResult.ToLower().Contains("delivered"))
                {
                    order.Status = OrderStatus.Delivered;
                }
                else if (request.AttemptResult.ToLower().Contains("failed"))
                {
                    order.Status = OrderStatus.Returned;
                }

                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedById = userId;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Delivery attempt recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording delivery attempt");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<DeliveryExceptionDto>>> GetDeliveryExceptionsAsync(long? orderId = null)
        {
            try
            {
                // In a real system, you'd have a DeliveryException entity
                // For now, we'll retrieve from audit logs
                var query = _context.AuditLogs
                    .Where(al => al.Action == "Delivery exception reported")
                    .AsNoTracking();

                if (orderId.HasValue)
                {
                    query = query.Where(al => al.Meta.Contains($"\"OrderId\":{orderId.Value}"));
                }

                var logs = await query
                    .OrderByDescending(al => al.OccurredAt)
                    .Take(100)
                    .ToListAsync();

                var exceptions = new List<DeliveryExceptionDto>();
                foreach (var log in logs)
                {
                    var user = await _userManager.FindByIdAsync(log.UserId);

                    // Parse metadata (simplified - in real system use proper deserialization)
                    exceptions.Add(new DeliveryExceptionDto
                    {
                        Id = log.Id,
                        OrderId = orderId ?? 0, // Would parse from Meta
                        ExceptionType = "Exception", // Would parse from Meta
                        Description = log.Meta,
                        ReportedById = log.UserId,
                        ReportedByName = user?.FullName,
                        ReportedAt = log.OccurredAt
                    });
                }

                return ApiResponse<List<DeliveryExceptionDto>>.SuccessResponse(exceptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery exceptions");
                return ApiResponse<List<DeliveryExceptionDto>>.ErrorResponse("An error occurred");
            }
        }
    }
}
