using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Inventory;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<InventoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ApiResponse<InventoryDto>> GetInventoryByIdAsync(long id)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                    .Include(i => i.Warehouse)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (inventory == null)
                {
                    return ApiResponse<InventoryDto>.ErrorResponse("Inventory record not found");
                }

                var inventoryDto = _mapper.Map<InventoryDto>(inventory);
                inventoryDto.Status = GetStockStatus(inventory);

                return ApiResponse<InventoryDto>.SuccessResponse(inventoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory by ID {Id}", id);
                return ApiResponse<InventoryDto>.ErrorResponse("An error occurred while retrieving inventory");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<InventoryDto>>> GetInventoriesAsync(InventoryQueryDto query)
        {
            try
            {
                var inventoriesQuery = _context.Inventories
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                    .Include(i => i.Warehouse)
                    .AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchLower = query.SearchTerm.ToLower();
                    inventoriesQuery = inventoriesQuery.Where(i =>
                        i.Product.Name.ToLower().Contains(searchLower) ||
                        i.Product.Sku.ToLower().Contains(searchLower) ||
                        i.Warehouse.Name.ToLower().Contains(searchLower));
                }

                if (query.WarehouseId.HasValue)
                {
                    inventoriesQuery = inventoriesQuery.Where(i => i.WarehouseId == query.WarehouseId.Value);
                }

                if (query.ProductId.HasValue)
                {
                    inventoriesQuery = inventoriesQuery.Where(i => i.ProductId == query.ProductId.Value);
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "All")
                {
                    inventoriesQuery = query.Status switch
                    {
                        "Out of Stock" => inventoriesQuery.Where(i => i.StockLevel == 0),
                        "Low Stock" => inventoriesQuery.Where(i => i.StockLevel > 0 && i.StockLevel <= i.ReorderLevel),
                        "Overstocked" => inventoriesQuery.Where(i => i.StockLevel >= i.MaxStock * 0.9m),
                        "In Stock" => inventoriesQuery.Where(i => i.StockLevel > i.ReorderLevel && i.StockLevel < i.MaxStock * 0.9m),
                        _ => inventoriesQuery
                    };
                }

                // Apply sorting
                inventoriesQuery = query.SortBy.ToLower() switch
                {
                    "productname" => query.SortDescending
                        ? inventoriesQuery.OrderByDescending(i => i.Product.Name)
                        : inventoriesQuery.OrderBy(i => i.Product.Name),
                    "warehouse" => query.SortDescending
                        ? inventoriesQuery.OrderByDescending(i => i.Warehouse.Name)
                        : inventoriesQuery.OrderBy(i => i.Warehouse.Name),
                    "stocklevel" => query.SortDescending
                        ? inventoriesQuery.OrderByDescending(i => i.StockLevel)
                        : inventoriesQuery.OrderBy(i => i.StockLevel),
                    "lastrestocked" => query.SortDescending
                        ? inventoriesQuery.OrderByDescending(i => i.LastRestocked)
                        : inventoriesQuery.OrderBy(i => i.LastRestocked),
                    _ => inventoriesQuery.OrderBy(i => i.Product.Name)
                };

                var totalCount = await inventoriesQuery.CountAsync();
                var inventories = await inventoriesQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                // Map using AutoMapper and set status
                var inventoryDtos = inventories.Select(i =>
                {
                    var dto = _mapper.Map<InventoryDto>(i);
                    dto.Status = GetStockStatus(i);
                    return dto;
                }).ToList();

                var paginatedResponse = new PaginatedResponse<InventoryDto>(
                    inventoryDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<InventoryDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventories");
                return ApiResponse<PaginatedResponse<InventoryDto>>.ErrorResponse("An error occurred while retrieving inventories");
            }
        }

        public async Task<ApiResponse<InventoryDto>> CreateInventoryAsync(CreateInventoryDto request, string userId)
        {
            try
            {
                // Check if product exists
                var product = await _context.Products.FindAsync(request.ProductId);
                if (product == null)
                {
                    return ApiResponse<InventoryDto>.ErrorResponse("Product not found");
                }

                // Check if warehouse exists
                var warehouse = await _context.Warehouses.FindAsync(request.WarehouseId);
                if (warehouse == null)
                {
                    return ApiResponse<InventoryDto>.ErrorResponse("Warehouse not found");
                }

                // Check if inventory already exists
                var existingInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId);

                if (existingInventory != null)
                {
                    return ApiResponse<InventoryDto>.ErrorResponse(
                        "Inventory record already exists for this product in this warehouse");
                }

                // Use AutoMapper to create inventory
                var inventory = _mapper.Map<Inventory>(request);
                inventory.LastRestocked = request.InitialStock > 0 ? DateTime.UtcNow : null;
                inventory.CreatedById = userId;
                inventory.UpdatedById = userId;

                _context.Inventories.Add(inventory);

                // Create initial movement if stock > 0
                if (request.InitialStock > 0)
                {
                    var movement = new InventoryMovement
                    {
                        Inventory = inventory,
                        MovementType = "Initial Stock",
                        Quantity = request.InitialStock,
                        PreviousStock = 0,
                        NewStock = request.InitialStock,
                        Notes = "Initial inventory setup",
                        MovementDate = DateTime.UtcNow,
                        CreatedById = userId,
                        UpdatedById = userId
                    };
                    _context.InventoryMovements.Add(movement);
                }

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Inventory created",
                    new
                    {
                        InventoryId = inventory.Id,
                        ProductId = request.ProductId,
                        WarehouseId = request.WarehouseId,
                        InitialStock = request.InitialStock
                    });

                // Reload with includes
                inventory = await _context.Inventories
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                    .Include(i => i.Warehouse)
                    .FirstAsync(i => i.Id == inventory.Id);

                var inventoryDto = _mapper.Map<InventoryDto>(inventory);
                inventoryDto.Status = GetStockStatus(inventory);

                return ApiResponse<InventoryDto>.SuccessResponse(
                    inventoryDto,
                    "Inventory created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory");
                return ApiResponse<InventoryDto>.ErrorResponse("An error occurred while creating inventory");
            }
        }

        public async Task<ApiResponse<InventoryDto>> AdjustInventoryAsync(AdjustInventoryDto request, string userId)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                    .Include(i => i.Warehouse)
                    .FirstOrDefaultAsync(i => i.Id == request.InventoryId);

                if (inventory == null)
                {
                    return ApiResponse<InventoryDto>.ErrorResponse("Inventory record not found");
                }

                var previousStock = inventory.StockLevel;
                var newStock = previousStock + request.QuantityAdjustment;

                // Prevent negative stock
                if (newStock < 0)
                {
                    return ApiResponse<InventoryDto>.ErrorResponse(
                        "Adjustment would result in negative stock",
                        new List<string> { $"Current stock: {previousStock}, Adjustment: {request.QuantityAdjustment}" });
                }

                inventory.StockLevel = newStock;
                inventory.UpdatedAt = DateTime.UtcNow;
                inventory.UpdatedById = userId;

                // Update last restocked if adding stock
                if (request.QuantityAdjustment > 0 && request.MovementType == "Restock")
                {
                    inventory.LastRestocked = DateTime.UtcNow;
                }

                // Create movement record using AutoMapper
                var movement = _mapper.Map<InventoryMovement>(request);
                movement.PreviousStock = previousStock;
                movement.NewStock = newStock;
                movement.CreatedById = userId;
                movement.UpdatedById = userId;

                _context.InventoryMovements.Add(movement);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Inventory adjusted",
                    new
                    {
                        InventoryId = inventory.Id,
                        ProductName = inventory.Product.Name,
                        WarehouseName = inventory.Warehouse.Name,
                        MovementType = request.MovementType,
                        Adjustment = request.QuantityAdjustment,
                        PreviousStock = previousStock,
                        NewStock = newStock
                    });

                var inventoryDto = _mapper.Map<InventoryDto>(inventory);
                inventoryDto.Status = GetStockStatus(inventory);

                return ApiResponse<InventoryDto>.SuccessResponse(
                    inventoryDto,
                    "Inventory adjusted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting inventory");
                return ApiResponse<InventoryDto>.ErrorResponse("An error occurred while adjusting inventory");
            }
        }

        public async Task<ApiResponse<InventoryDto>> RestockInventoryAsync(RestockInventoryDto request, string userId)
        {
            try
            {
                // Find or create inventory record
                var inventory = await _context.Inventories
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                    .Include(i => i.Warehouse)
                    .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId);

                if (inventory == null)
                {
                    // Auto-create inventory if it doesn't exist
                    inventory = new Inventory
                    {
                        ProductId = request.ProductId,
                        WarehouseId = request.WarehouseId,
                        StockLevel = 0,
                        ReorderLevel = 50,
                        MaxStock = 1000,
                        CreatedById = userId,
                        UpdatedById = userId
                    };
                    _context.Inventories.Add(inventory);
                    await _context.SaveChangesAsync();

                    // Reload with includes
                    inventory = await _context.Inventories
                        .Include(i => i.Product)
                            .ThenInclude(p => p.Category)
                        .Include(i => i.Warehouse)
                        .FirstAsync(i => i.Id == inventory.Id);
                }

                var previousStock = inventory.StockLevel;
                inventory.StockLevel += request.Quantity;
                inventory.LastRestocked = DateTime.UtcNow;
                inventory.UpdatedAt = DateTime.UtcNow;
                inventory.UpdatedById = userId;

                // Create movement record
                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    MovementType = "Restock",
                    Quantity = request.Quantity,
                    PreviousStock = previousStock,
                    NewStock = inventory.StockLevel,
                    Reference = request.Reference,
                    Notes = request.Notes,
                    MovementDate = DateTime.UtcNow,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                _context.InventoryMovements.Add(movement);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Inventory restocked",
                    new
                    {
                        InventoryId = inventory.Id,
                        ProductName = inventory.Product.Name,
                        WarehouseName = inventory.Warehouse.Name,
                        Quantity = request.Quantity,
                        PreviousStock = previousStock,
                        NewStock = inventory.StockLevel
                    });

                var inventoryDto = _mapper.Map<InventoryDto>(inventory);
                inventoryDto.Status = GetStockStatus(inventory);

                return ApiResponse<InventoryDto>.SuccessResponse(
                    inventoryDto,
                    "Inventory restocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restocking inventory");
                return ApiResponse<InventoryDto>.ErrorResponse("An error occurred while restocking inventory");
            }
        }

        public async Task<ApiResponse<InventoryDto>> UpdateInventoryLevelsAsync(UpdateInventoryLevelsDto request, string userId)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Product)
                        .ThenInclude(p => p.Category)
                    .Include(i => i.Warehouse)
                    .FirstOrDefaultAsync(i => i.Id == request.InventoryId);

                if (inventory == null)
                {
                    return ApiResponse<InventoryDto>.ErrorResponse("Inventory record not found");
                }

                inventory.ReorderLevel = request.ReorderLevel;
                inventory.MaxStock = request.MaxStock;
                inventory.UpdatedAt = DateTime.UtcNow;
                inventory.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Inventory levels updated",
                    new
                    {
                        InventoryId = inventory.Id,
                        ProductName = inventory.Product.Name,
                        ReorderLevel = request.ReorderLevel,
                        MaxStock = request.MaxStock
                    });

                var inventoryDto = _mapper.Map<InventoryDto>(inventory);
                inventoryDto.Status = GetStockStatus(inventory);

                return ApiResponse<InventoryDto>.SuccessResponse(
                    inventoryDto,
                    "Inventory levels updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory levels");
                return ApiResponse<InventoryDto>.ErrorResponse("An error occurred while updating inventory levels");
            }
        }

        public async Task<ApiResponse<List<InventoryMovementDto>>> GetInventoryMovementsAsync(long inventoryId, int limit = 50)
        {
            try
            {
                var movements = await _context.InventoryMovements
                    .Include(m => m.Inventory)
                        .ThenInclude(i => i.Product)
                    .Include(m => m.Inventory)
                        .ThenInclude(i => i.Warehouse)
                    .Where(m => m.InventoryId == inventoryId)
                    .OrderByDescending(m => m.MovementDate)
                    .Take(limit)
                    .AsNoTracking()
                    .ToListAsync();

                // Use AutoMapper
                var movementDtos = _mapper.Map<List<InventoryMovementDto>>(movements);

                return ApiResponse<List<InventoryMovementDto>>.SuccessResponse(movementDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory movements");
                return ApiResponse<List<InventoryMovementDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<InventorySummaryDto>> GetInventorySummaryAsync(long? warehouseId = null)
        {
            try
            {
                var query = _context.Inventories
                    .Include(i => i.Product)
                    .AsNoTracking();

                if (warehouseId.HasValue)
                {
                    query = query.Where(i => i.WarehouseId == warehouseId.Value);
                }

                var inventories = await query.ToListAsync();

                var summary = new InventorySummaryDto
                {
                    TotalProducts = inventories.Count,
                    InStock = inventories.Count(i => i.StockLevel > i.ReorderLevel && i.StockLevel < i.MaxStock * 0.9m),
                    LowStock = inventories.Count(i => i.StockLevel > 0 && i.StockLevel <= i.ReorderLevel),
                    OutOfStock = inventories.Count(i => i.StockLevel == 0),
                    Overstocked = inventories.Count(i => i.StockLevel >= i.MaxStock * 0.9m),
                    TotalInventoryValue = inventories.Sum(i => i.StockLevel * i.Product.Price)
                };

                return ApiResponse<InventorySummaryDto>.SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory summary");
                return ApiResponse<InventorySummaryDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> DeleteInventoryAsync(long id)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Movements)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (inventory == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Inventory record not found");
                }

                // Check if there's current stock
                if (inventory.StockLevel > 0)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete inventory with stock",
                        new List<string> { $"Current stock level: {inventory.StockLevel}. Adjust to zero before deleting." });
                }

                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Inventory deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting inventory");
            }
        }

        // Helper method to determine stock status
        private string GetStockStatus(Inventory inventory)
        {
            if (inventory.StockLevel == 0)
                return "Out of Stock";
            if (inventory.StockLevel <= inventory.ReorderLevel)
                return "Low Stock";
            if (inventory.StockLevel >= inventory.MaxStock * 0.9m)
                return "Overstocked";
            return "In Stock";
        }
    }
}
