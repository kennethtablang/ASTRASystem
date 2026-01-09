using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<WarehouseService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(long id)
        {
            try
            {
                var warehouse = await _context.Warehouses
                    .Include(w => w.Distributor)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (warehouse == null)
                {
                    return ApiResponse<WarehouseDto>.ErrorResponse("Warehouse not found");
                }

                var warehouseDto = _mapper.Map<WarehouseDto>(warehouse);
                return ApiResponse<WarehouseDto>.SuccessResponse(warehouseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouse by ID {Id}", id);
                return ApiResponse<WarehouseDto>.ErrorResponse("An error occurred while retrieving warehouse");
            }
        }

        public async Task<ApiResponse<List<WarehouseDto>>> GetWarehousesAsync(long? distributorId = null)
        {
            try
            {
                var query = _context.Warehouses
                    .Include(w => w.Distributor)
                    .AsNoTracking();

                if (distributorId.HasValue)
                {
                    query = query.Where(w => w.DistributorId == distributorId.Value);
                }

                var warehouses = await query
                    .OrderBy(w => w.Name)
                    .ToListAsync();

                var warehouseDtos = _mapper.Map<List<WarehouseDto>>(warehouses);
                return ApiResponse<List<WarehouseDto>>.SuccessResponse(warehouseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting warehouses");
                return ApiResponse<List<WarehouseDto>>.ErrorResponse("An error occurred while retrieving warehouses");
            }
        }

        public async Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto request, string userId)
        {
            try
            {
                // Validate distributor exists
                var distributorExists = await _context.Distributors
                    .AnyAsync(d => d.Id == request.DistributorId);

                if (!distributorExists)
                {
                    return ApiResponse<WarehouseDto>.ErrorResponse("Distributor not found");
                }

                // Check if warehouse name already exists for this distributor
                var duplicateName = await _context.Warehouses
                    .AnyAsync(w => w.DistributorId == request.DistributorId &&
                                   w.Name.ToLower() == request.Name.ToLower());

                if (duplicateName)
                {
                    return ApiResponse<WarehouseDto>.ErrorResponse(
                        "A warehouse with this name already exists for this distributor");
                }

                var warehouse = _mapper.Map<Warehouse>(request);
                warehouse.CreatedAt = DateTime.UtcNow;
                warehouse.UpdatedAt = DateTime.UtcNow;
                warehouse.CreatedById = userId;
                warehouse.UpdatedById = userId;
                warehouse.IsActive = request.IsActive;

                _context.Warehouses.Add(warehouse);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Warehouse created",
                    new { WarehouseId = warehouse.Id, Name = warehouse.Name });

                // Reload with distributor info
                warehouse = await _context.Warehouses
                    .Include(w => w.Distributor)
                    .FirstAsync(w => w.Id == warehouse.Id);

                var warehouseDto = _mapper.Map<WarehouseDto>(warehouse);
                return ApiResponse<WarehouseDto>.SuccessResponse(
                    warehouseDto,
                    "Warehouse created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse");
                return ApiResponse<WarehouseDto>.ErrorResponse("An error occurred while creating warehouse");
            }
        }

        public async Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(WarehouseDto request, string userId)
        {
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(request.Id);
                if (warehouse == null)
                {
                    return ApiResponse<WarehouseDto>.ErrorResponse("Warehouse not found");
                }

                // Validate distributor exists
                var distributorExists = await _context.Distributors
                    .AnyAsync(d => d.Id == request.DistributorId);

                if (!distributorExists)
                {
                    return ApiResponse<WarehouseDto>.ErrorResponse("Distributor not found");
                }

                // Check if name already exists (excluding current warehouse)
                var duplicateName = await _context.Warehouses
                    .AnyAsync(w => w.DistributorId == request.DistributorId &&
                                   w.Name.ToLower() == request.Name.ToLower() &&
                                   w.Id != request.Id);

                if (duplicateName)
                {
                    return ApiResponse<WarehouseDto>.ErrorResponse(
                        "A warehouse with this name already exists for this distributor");
                }

                warehouse.DistributorId = request.DistributorId;
                warehouse.Name = request.Name;
                warehouse.Address = request.Address;
                warehouse.Latitude = request.Latitude;
                warehouse.Longitude = request.Longitude;
                warehouse.IsActive = request.IsActive;
                warehouse.UpdatedAt = DateTime.UtcNow;
                warehouse.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Warehouse updated",
                    new { WarehouseId = warehouse.Id, Name = warehouse.Name });

                // Reload with distributor info
                warehouse = await _context.Warehouses
                    .Include(w => w.Distributor)
                    .FirstAsync(w => w.Id == warehouse.Id);

                var warehouseDto = _mapper.Map<WarehouseDto>(warehouse);
                return ApiResponse<WarehouseDto>.SuccessResponse(
                    warehouseDto,
                    "Warehouse updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse");
                return ApiResponse<WarehouseDto>.ErrorResponse("An error occurred while updating warehouse");
            }
        }

        public async Task<ApiResponse<bool>> DeleteWarehouseAsync(long id)
        {
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(id);
                if (warehouse == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Warehouse not found");
                }

                // Check if warehouse has associated orders
                var hasOrders = await _context.Orders.AnyAsync(o => o.WarehouseId == id);
                if (hasOrders)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete warehouse with existing orders");
                }

                // Check if warehouse has associated trips
                var hasTrips = await _context.Trips.AnyAsync(t => t.WarehouseId == id);
                if (hasTrips)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete warehouse with existing trips");
                }

                // Check if any users are assigned to this warehouse
                var hasUsers = await _context.Users.AnyAsync(u => u.WarehouseId == id);
                if (hasUsers)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete warehouse with assigned users",
                        new List<string> { "Please reassign users to another warehouse first." });
                }

                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Warehouse deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting warehouse");
            }
        }
    }
}
