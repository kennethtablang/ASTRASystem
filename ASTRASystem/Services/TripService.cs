using Microsoft.EntityFrameworkCore;
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Trip;
using ASTRASystem.Enum;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ASTRASystem.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TripService> _logger;

        public TripService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ILogger<TripService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApiResponse<TripDto>> GetTripByIdAsync(long id)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Warehouse)
                        .ThenInclude(w => w.Distributor)
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Store)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (trip == null)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Trip not found");
                }

                var tripDto = _mapper.Map<TripDto>(trip);

                if (!string.IsNullOrEmpty(trip.DispatcherId))
                {
                    var dispatcher = await _userManager.FindByIdAsync(trip.DispatcherId);
                    tripDto.DispatcherName = dispatcher?.FullName;
                }

                return ApiResponse<TripDto>.SuccessResponse(tripDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trip by ID {Id}", id);
                return ApiResponse<TripDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<TripListItemDto>>> GetTripsAsync(TripQueryDto query)
        {
            try
            {
                var tripsQuery = _context.Trips
                    .Include(t => t.Warehouse)
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                    .AsNoTracking();

                if (query.Status.HasValue)
                {
                    tripsQuery = tripsQuery.Where(t => t.Status == query.Status.Value);
                }

                if (query.WarehouseId.HasValue)
                {
                    tripsQuery = tripsQuery.Where(t => t.WarehouseId == query.WarehouseId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.DispatcherId))
                {
                    tripsQuery = tripsQuery.Where(t => t.DispatcherId == query.DispatcherId);
                }

                if (query.DepartureFrom.HasValue)
                {
                    tripsQuery = tripsQuery.Where(t => t.DepartureAt >= query.DepartureFrom.Value);
                }

                if (query.DepartureTo.HasValue)
                {
                    tripsQuery = tripsQuery.Where(t => t.DepartureAt <= query.DepartureTo.Value);
                }

                tripsQuery = query.SortBy.ToLower() switch
                {
                    "departureat" => query.SortDescending
                        ? tripsQuery.OrderByDescending(t => t.DepartureAt)
                        : tripsQuery.OrderBy(t => t.DepartureAt),
                    "status" => query.SortDescending
                        ? tripsQuery.OrderByDescending(t => t.Status)
                        : tripsQuery.OrderBy(t => t.Status),
                    _ => query.SortDescending
                        ? tripsQuery.OrderByDescending(t => t.DepartureAt)
                        : tripsQuery.OrderBy(t => t.DepartureAt)
                };

                var totalCount = await tripsQuery.CountAsync();
                var trips = await tripsQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var tripDtos = new List<TripListItemDto>();
                foreach (var trip in trips)
                {
                    var dto = _mapper.Map<TripListItemDto>(trip);

                    if (!string.IsNullOrEmpty(trip.DispatcherId))
                    {
                        var dispatcher = await _userManager.FindByIdAsync(trip.DispatcherId);
                        dto.DispatcherName = dispatcher?.FullName;
                    }

                    dto.TotalValue = trip.Assignments.Sum(a => a.Order.Total);
                    tripDtos.Add(dto);
                }

                var paginatedResponse = new PaginatedResponse<TripListItemDto>(
                    tripDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<TripListItemDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trips");
                return ApiResponse<PaginatedResponse<TripListItemDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<TripDto>> CreateTripAsync(CreateTripDto request, string userId)
        {
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId);
                if (warehouse == null)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Warehouse not found");
                }

                var dispatcher = await _userManager.FindByIdAsync(request.DispatcherId);
                if (dispatcher == null)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Dispatcher not found");
                }

                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Where(o => request.OrderIds.Contains(o.Id) &&
                                o.Status == OrderStatus.Packed &&
                                o.WarehouseId == request.WarehouseId)
                    .ToListAsync();

                if (orders.Count != request.OrderIds.Count)
                {
                    return ApiResponse<TripDto>.ErrorResponse(
                        "Some orders were not found or are not ready for dispatch");
                }

                var trip = new Trip
                {
                    WarehouseId = request.WarehouseId,
                    DispatcherId = request.DispatcherId,
                    Status = TripStatus.Created,
                    DepartureAt = request.DepartureAt,
                    Vehicle = request.Vehicle,
                    EstimatedReturn = request.EstimatedReturn,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                int sequenceNo = 1;
                foreach (var orderId in request.OrderIds)
                {
                    var assignment = new TripAssignment
                    {
                        TripId = trip.Id,
                        OrderId = orderId,
                        SequenceNo = sequenceNo++,
                        Status = OrderStatus.Dispatched,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = userId,
                        UpdatedById = userId
                    };
                    _context.TripAssignments.Add(assignment);

                    var order = orders.First(o => o.Id == orderId);
                    order.Status = OrderStatus.Dispatched;
                    order.UpdatedAt = DateTime.UtcNow;
                    order.UpdatedById = userId;
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Trip created",
                    new { TripId = trip.Id, OrderCount = request.OrderIds.Count });

                await _notificationService.SendNotificationAsync(
                    request.DispatcherId,
                    "TripAssigned",
                    $"You have been assigned to trip #{trip.Id} with {request.OrderIds.Count} orders");

                return await GetTripByIdAsync(trip.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                return ApiResponse<TripDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<TripDto>> UpdateTripAsync(UpdateTripDto request, string userId)
        {
            try
            {
                var trip = await _context.Trips.FindAsync(request.TripId);
                if (trip == null)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Trip not found");
                }

                if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Cannot update completed or cancelled trips");
                }

                if (!string.IsNullOrEmpty(request.DispatcherId))
                {
                    var dispatcher = await _userManager.FindByIdAsync(request.DispatcherId);
                    if (dispatcher == null)
                    {
                        return ApiResponse<TripDto>.ErrorResponse("Dispatcher not found");
                    }
                    trip.DispatcherId = request.DispatcherId;
                }

                trip.DepartureAt = request.DepartureAt ?? trip.DepartureAt;
                trip.Vehicle = request.Vehicle ?? trip.Vehicle;
                trip.EstimatedReturn = request.EstimatedReturn ?? trip.EstimatedReturn;
                trip.UpdatedAt = DateTime.UtcNow;
                trip.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(userId, "Trip updated", new { TripId = trip.Id });

                return await GetTripByIdAsync(trip.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip");
                return ApiResponse<TripDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<TripDto>> UpdateTripStatusAsync(UpdateTripStatusDto request, string userId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Assignments)
                    .FirstOrDefaultAsync(t => t.Id == request.TripId);

                if (trip == null)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Trip not found");
                }

                var oldStatus = trip.Status;
                trip.Status = request.NewStatus;
                trip.UpdatedAt = DateTime.UtcNow;
                trip.UpdatedById = userId;

                if (request.NewStatus == TripStatus.Started)
                {
                    foreach (var assignment in trip.Assignments)
                    {
                        var order = await _context.Orders.FindAsync(assignment.OrderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.InTransit;
                            order.UpdatedAt = DateTime.UtcNow;
                            order.UpdatedById = userId;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Trip status updated",
                    new
                    {
                        TripId = trip.Id,
                        OldStatus = oldStatus.ToString(),
                        NewStatus = request.NewStatus.ToString(),
                        Notes = request.Notes
                    });

                return await GetTripByIdAsync(trip.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip status");
                return ApiResponse<TripDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ReorderTripAssignmentsAsync(ReorderTripAssignmentsDto request, string userId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Assignments)
                    .FirstOrDefaultAsync(t => t.Id == request.TripId);

                if (trip == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Trip not found");
                }

                if (trip.Status != TripStatus.Created && trip.Status != TripStatus.Assigned)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Can only reorder assignments for trips that haven't started");
                }

                foreach (var sequence in request.Sequences)
                {
                    var assignment = trip.Assignments.FirstOrDefault(a => a.OrderId == sequence.OrderId);
                    if (assignment != null)
                    {
                        assignment.SequenceNo = sequence.SequenceNo;
                        assignment.UpdatedAt = DateTime.UtcNow;
                        assignment.UpdatedById = userId;
                    }
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Trip assignments reordered",
                    new { TripId = trip.Id });

                return ApiResponse<bool>.SuccessResponse(true, "Trip assignments reordered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering trip assignments");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> CancelTripAsync(long tripId, string userId, string? reason)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Assignments)
                    .FirstOrDefaultAsync(t => t.Id == tripId);

                if (trip == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Trip not found");
                }

                if (trip.Status == TripStatus.Completed || trip.Status == TripStatus.Cancelled)
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot cancel completed or already cancelled trips");
                }

                trip.Status = TripStatus.Cancelled;
                trip.UpdatedAt = DateTime.UtcNow;
                trip.UpdatedById = userId;

                foreach (var assignment in trip.Assignments)
                {
                    var order = await _context.Orders.FindAsync(assignment.OrderId);
                    if (order != null && order.Status == OrderStatus.Dispatched)
                    {
                        order.Status = OrderStatus.Packed;
                        order.UpdatedAt = DateTime.UtcNow;
                        order.UpdatedById = userId;
                    }
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Trip cancelled",
                    new { TripId = trip.Id, Reason = reason });

                return ApiResponse<bool>.SuccessResponse(true, "Trip cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling trip");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<TripManifestDto>> GetTripManifestAsync(long tripId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Warehouse)
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Store)
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Items)
                                .ThenInclude(i => i.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tripId);

                if (trip == null)
                {
                    return ApiResponse<TripManifestDto>.ErrorResponse("Trip not found");
                }

                var dispatcher = await _userManager.FindByIdAsync(trip.DispatcherId);

                var manifest = new TripManifestDto
                {
                    TripId = trip.Id,
                    WarehouseName = trip.Warehouse.Name,
                    WarehouseAddress = trip.Warehouse.Address,
                    DispatcherName = dispatcher?.FullName ?? "Unknown",
                    Vehicle = trip.Vehicle,
                    DepartureAt = trip.DepartureAt,
                    TotalOrders = trip.Assignments.Count,
                    TotalValue = trip.Assignments.Sum(a => a.Order.Total),
                    GeneratedAt = DateTime.UtcNow
                };

                var stops = trip.Assignments.OrderBy(a => a.SequenceNo).Select(assignment =>
                {
                    var order = assignment.Order;
                    return new ManifestStopDto
                    {
                        SequenceNo = assignment.SequenceNo,
                        OrderId = order.Id,
                        StoreName = order.Store.Name,
                        StoreAddress = $"{order.Store.Barangay}, {order.Store.City}",
                        StorePhone = order.Store.Phone,
                        StoreOwner = order.Store.OwnerName,
                        OrderTotal = order.Total,
                        Items = order.Items.Select(item => new ManifestItemDto
                        {
                            Sku = item.Product.Sku,
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitOfMeasure = item.Product.UnitOfMeasure
                        }).ToList()
                    };
                }).ToList();

                manifest.Stops = stops;

                return ApiResponse<TripManifestDto>.SuccessResponse(manifest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trip manifest");
                return ApiResponse<TripManifestDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<byte[]>> GenerateTripManifestPdfAsync(long tripId)
        {
            try
            {
                var manifestResponse = await GetTripManifestAsync(tripId);
                if (!manifestResponse.Success)
                {
                    return ApiResponse<byte[]>.ErrorResponse(manifestResponse.Message);
                }

                // This will be implemented in PdfService
                return ApiResponse<byte[]>.ErrorResponse("PDF generation not yet implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trip manifest PDF");
                return ApiResponse<byte[]>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<TripDto>>> GetActiveTripsAsync(string? dispatcherId = null)
        {
            try
            {
                var query = _context.Trips
                    .Include(t => t.Warehouse)
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Store)
                    .Where(t => t.Status == TripStatus.Started || t.Status == TripStatus.InProgress)
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(dispatcherId))
                {
                    query = query.Where(t => t.DispatcherId == dispatcherId);
                }

                var trips = await query.ToListAsync();

                var tripDtos = new List<TripDto>();
                foreach (var trip in trips)
                {
                    var dto = _mapper.Map<TripDto>(trip);
                    if (!string.IsNullOrEmpty(trip.DispatcherId))
                    {
                        var dispatcher = await _userManager.FindByIdAsync(trip.DispatcherId);
                        dto.DispatcherName = dispatcher?.FullName;
                    }
                    tripDtos.Add(dto);
                }

                return ApiResponse<List<TripDto>>.SuccessResponse(tripDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active trips");
                return ApiResponse<List<TripDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<long>>> SuggestTripSequenceAsync(List<long> orderIds)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Store)
                    .Where(o => orderIds.Contains(o.Id))
                    .ToListAsync();

                // Simple algorithm: group by city, then by barangay, then by priority
                var orderedOrders = orders
                    .OrderBy(o => o.Store.City)
                    .ThenBy(o => o.Store.Barangay)
                    .ThenByDescending(o => o.Priority)
                    .Select(o => o.Id)
                    .ToList();

                return ApiResponse<List<long>>.SuccessResponse(orderedOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting trip sequence");
                return ApiResponse<List<long>>.ErrorResponse("An error occurred");
            }
        }
    }
}
