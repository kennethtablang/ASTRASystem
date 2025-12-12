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
        private readonly IPdfService _pdfService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TripService> _logger;

        public TripService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            INotificationService notificationService,
            IPdfService pdfService,
            UserManager<ApplicationUser> userManager,
            ILogger<TripService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
            _pdfService = pdfService;
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
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Payments)
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
                    .AsNoTracking();

                // Apply filters
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

                // Apply sorting
                tripsQuery = query.SortBy.ToLower() switch
                {
                    "createdat" => query.SortDescending
                        ? tripsQuery.OrderByDescending(t => t.CreatedAt)
                        : tripsQuery.OrderBy(t => t.CreatedAt),
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

                    // Calculate total value from assignments
                    var orderIds = trip.Assignments.Select(a => a.OrderId).ToList();
                    var orders = await _context.Orders
                        .Where(o => orderIds.Contains(o.Id))
                        .ToListAsync();
                    dto.TotalValue = orders.Sum(o => o.Total);

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
                // Validate warehouse
                var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId);
                if (!warehouseExists)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Warehouse not found");
                }

                // Validate dispatcher
                var dispatcher = await _userManager.FindByIdAsync(request.DispatcherId);
                if (dispatcher == null)
                {
                    return ApiResponse<TripDto>.ErrorResponse("Dispatcher not found");
                }

                // Validate orders exist and are packed
                var orders = await _context.Orders
                    .Where(o => request.OrderIds.Contains(o.Id))
                    .ToListAsync();

                if (orders.Count != request.OrderIds.Count)
                {
                    return ApiResponse<TripDto>.ErrorResponse("One or more orders not found");
                }

                var notPackedOrders = orders.Where(o => o.Status != OrderStatus.Packed).ToList();
                if (notPackedOrders.Any())
                {
                    return ApiResponse<TripDto>.ErrorResponse(
                        "All orders must be in 'Packed' status",
                        notPackedOrders.Select(o => $"Order #{o.Id} is {o.Status}").ToList());
                }

                // Create trip
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

                // Create trip assignments
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

                    // Update order status
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

                // Notify dispatcher
                await _notificationService.SendNotificationAsync(
                    request.DispatcherId,
                    "TripAssigned",
                    $"Trip #{trip.Id} has been assigned to you with {request.OrderIds.Count} order(s)");

                return await GetTripByIdAsync(trip.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                return ApiResponse<TripDto>.ErrorResponse("An error occurred while creating trip");
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
                    return ApiResponse<TripDto>.ErrorResponse(
                        "Cannot update completed or cancelled trips");
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

                if (request.DepartureAt.HasValue)
                {
                    trip.DepartureAt = request.DepartureAt;
                }

                if (!string.IsNullOrEmpty(request.Vehicle))
                {
                    trip.Vehicle = request.Vehicle;
                }

                if (request.EstimatedReturn.HasValue)
                {
                    trip.EstimatedReturn = request.EstimatedReturn;
                }

                trip.UpdatedAt = DateTime.UtcNow;
                trip.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Trip updated",
                    new { TripId = trip.Id });

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

                // If starting trip, update order statuses
                if (request.NewStatus == TripStatus.Started || request.NewStatus == TripStatus.InProgress)
                {
                    var orderIds = trip.Assignments.Select(a => a.OrderId).ToList();
                    var orders = await _context.Orders
                        .Where(o => orderIds.Contains(o.Id))
                        .ToListAsync();

                    foreach (var order in orders)
                    {
                        order.Status = OrderStatus.InTransit;
                        order.UpdatedAt = DateTime.UtcNow;
                        order.UpdatedById = userId;
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

                // Notify dispatcher
                if (!string.IsNullOrEmpty(trip.DispatcherId))
                {
                    await _notificationService.SendNotificationAsync(
                        trip.DispatcherId,
                        "TripStatusChanged",
                        $"Trip #{trip.Id} status changed to {request.NewStatus}");
                }

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
                        "Can only reorder trips that haven't started");
                }

                // Validate all orders belong to this trip
                var tripOrderIds = trip.Assignments.Select(a => a.OrderId).ToHashSet();
                var requestOrderIds = request.Sequences.Select(s => s.OrderId).ToHashSet();

                if (!tripOrderIds.SetEquals(requestOrderIds))
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Sequence must include all trip orders");
                }

                // Update sequence numbers
                foreach (var sequence in request.Sequences)
                {
                    var assignment = trip.Assignments.First(a => a.OrderId == sequence.OrderId);
                    assignment.SequenceNo = sequence.SequenceNo;
                    assignment.UpdatedAt = DateTime.UtcNow;
                    assignment.UpdatedById = userId;
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Trip sequence reordered",
                    new { TripId = trip.Id });

                return ApiResponse<bool>.SuccessResponse(true, "Trip sequence updated successfully");
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
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot cancel completed or already cancelled trips");
                }

                trip.Status = TripStatus.Cancelled;
                trip.UpdatedAt = DateTime.UtcNow;
                trip.UpdatedById = userId;

                // Update order statuses back to Packed
                var orderIds = trip.Assignments.Select(a => a.OrderId).ToList();
                var orders = await _context.Orders
                    .Where(o => orderIds.Contains(o.Id))
                    .ToListAsync();

                foreach (var order in orders)
                {
                    order.Status = OrderStatus.Packed;
                    order.UpdatedAt = DateTime.UtcNow;
                    order.UpdatedById = userId;
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Trip cancelled",
                    new { TripId = tripId, Reason = reason });

                // Notify dispatcher
                if (!string.IsNullOrEmpty(trip.DispatcherId))
                {
                    await _notificationService.SendNotificationAsync(
                        trip.DispatcherId,
                        "TripCancelled",
                        $"Trip #{trip.Id} has been cancelled");
                }

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

                var dispatcher = !string.IsNullOrEmpty(trip.DispatcherId)
                    ? await _userManager.FindByIdAsync(trip.DispatcherId)
                    : null;

                var manifest = new TripManifestDto
                {
                    TripId = trip.Id,
                    WarehouseName = trip.Warehouse.Name,
                    WarehouseAddress = trip.Warehouse.Address,
                    DispatcherName = dispatcher?.FullName ?? "Unassigned",
                    Vehicle = trip.Vehicle,
                    DepartureAt = trip.DepartureAt,
                    GeneratedAt = DateTime.UtcNow
                };

                foreach (var assignment in trip.Assignments.OrderBy(a => a.SequenceNo))
                {
                    var stop = new ManifestStopDto
                    {
                        SequenceNo = assignment.SequenceNo,
                        OrderId = assignment.OrderId,
                        StoreName = assignment.Order.Store.Name,
                        StoreAddress = $"{assignment.Order.Store.Barangay}, {assignment.Order.Store.City}",
                        StorePhone = assignment.Order.Store.Phone,
                        StoreOwner = assignment.Order.Store.OwnerName,
                        OrderTotal = assignment.Order.Total
                    };

                    foreach (var item in assignment.Order.Items)
                    {
                        stop.Items.Add(new ManifestItemDto
                        {
                            Sku = item.Product.Sku,
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitOfMeasure = item.Product.UnitOfMeasure
                        });
                    }

                    manifest.Stops.Add(stop);
                }

                manifest.TotalOrders = manifest.Stops.Count;
                manifest.TotalValue = manifest.Stops.Sum(s => s.OrderTotal);

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
                var trip = await _context.Trips
                    .Include(t => t.Warehouse)
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Store)
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Items)
                                .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(t => t.Id == tripId);

                if (trip == null)
                {
                    return ApiResponse<byte[]>.ErrorResponse("Trip not found");
                }

                var pdfBytes = _pdfService.GenerateTripManifestPdf(trip);
                return ApiResponse<byte[]>.SuccessResponse(pdfBytes);
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
                    .Include(t => t.Assignments)
                        .ThenInclude(a => a.Order)
                            .ThenInclude(o => o.Payments)
                    .Where(t => t.Status == TripStatus.Started || t.Status == TripStatus.InProgress)
                    .AsNoTracking();

                if (!string.IsNullOrEmpty(dispatcherId))
                {
                    query = query.Where(t => t.DispatcherId == dispatcherId);
                }

                var trips = await query
                    .OrderBy(t => t.DepartureAt)
                    .ToListAsync();

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

                if (orders.Count != orderIds.Count)
                {
                    return ApiResponse<List<long>>.ErrorResponse("Some orders not found");
                }

                // Simple optimization: group by city/barangay, then priority
                var optimizedSequence = orders
                    .OrderBy(o => o.Store.City)
                    .ThenBy(o => o.Store.Barangay)
                    .ThenByDescending(o => o.Priority)
                    .ThenBy(o => o.CreatedAt)
                    .Select(o => o.Id)
                    .ToList();

                return ApiResponse<List<long>>.SuccessResponse(optimizedSequence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting trip sequence");
                return ApiResponse<List<long>>.ErrorResponse("An error occurred");
            }
        }
    }
}
