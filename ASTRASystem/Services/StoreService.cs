using Microsoft.EntityFrameworkCore;
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Store;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Services
{
    public class StoreService : IStoreService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<StoreService> _logger;

        public StoreService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<StoreService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ApiResponse<StoreDto>> GetStoreByIdAsync(long id)
        {
            try
            {
                var store = await _context.Stores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (store == null)
                {
                    return ApiResponse<StoreDto>.ErrorResponse("Store not found");
                }

                var storeDto = _mapper.Map<StoreDto>(store);
                return ApiResponse<StoreDto>.SuccessResponse(storeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting store by ID {Id}", id);
                return ApiResponse<StoreDto>.ErrorResponse("An error occurred while retrieving store");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<StoreDto>>> GetStoresAsync(StoreQueryDto query)
        {
            try
            {
                var storesQuery = _context.Stores.AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchLower = query.SearchTerm.ToLower();
                    storesQuery = storesQuery.Where(s =>
                        s.Name.ToLower().Contains(searchLower) ||
                        (s.OwnerName != null && s.OwnerName.ToLower().Contains(searchLower)) ||
                        (s.Phone != null && s.Phone.Contains(query.SearchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(query.Barangay))
                {
                    storesQuery = storesQuery.Where(s => s.Barangay == query.Barangay);
                }

                if (!string.IsNullOrWhiteSpace(query.City))
                {
                    storesQuery = storesQuery.Where(s => s.City == query.City);
                }

                // Apply sorting
                storesQuery = query.SortBy.ToLower() switch
                {
                    "name" => query.SortDescending
                        ? storesQuery.OrderByDescending(s => s.Name)
                        : storesQuery.OrderBy(s => s.Name),
                    "barangay" => query.SortDescending
                        ? storesQuery.OrderByDescending(s => s.Barangay)
                        : storesQuery.OrderBy(s => s.Barangay),
                    "city" => query.SortDescending
                        ? storesQuery.OrderByDescending(s => s.City)
                        : storesQuery.OrderBy(s => s.City),
                    _ => storesQuery.OrderBy(s => s.Name)
                };

                var totalCount = await storesQuery.CountAsync();
                var stores = await storesQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var storeDtos = _mapper.Map<List<StoreDto>>(stores);
                var paginatedResponse = new PaginatedResponse<StoreDto>(
                    storeDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<StoreDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stores");
                return ApiResponse<PaginatedResponse<StoreDto>>.ErrorResponse("An error occurred while retrieving stores");
            }
        }

        public async Task<ApiResponse<List<StoreListItemDto>>> GetStoresForLookupAsync(string? searchTerm = null)
        {
            try
            {
                var query = _context.Stores.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(s =>
                        s.Name.ToLower().Contains(searchLower) ||
                        (s.OwnerName != null && s.OwnerName.ToLower().Contains(searchLower)));
                }

                var stores = await query
                    .OrderBy(s => s.Name)
                    .Take(100) // Limit for lookup
                    .ToListAsync();

                var storeDtos = _mapper.Map<List<StoreListItemDto>>(stores);
                return ApiResponse<List<StoreListItemDto>>.SuccessResponse(storeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stores for lookup");
                return ApiResponse<List<StoreListItemDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<StoreDto>> CreateStoreAsync(CreateStoreDto request, string userId)
        {
            try
            {
                // Check if store name already exists
                var existingStore = await _context.Stores
                    .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower());

                if (existingStore)
                {
                    return ApiResponse<StoreDto>.ErrorResponse(
                        "A store with this name already exists");
                }

                var store = _mapper.Map<Store>(request);
                store.CreatedAt = DateTime.UtcNow;
                store.UpdatedAt = DateTime.UtcNow;
                store.CreatedById = userId;
                store.UpdatedById = userId;

                _context.Stores.Add(store);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Store created",
                    new { StoreId = store.Id, Name = store.Name });

                var storeDto = _mapper.Map<StoreDto>(store);
                return ApiResponse<StoreDto>.SuccessResponse(
                    storeDto,
                    "Store created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating store");
                return ApiResponse<StoreDto>.ErrorResponse("An error occurred while creating store");
            }
        }

        public async Task<ApiResponse<StoreDto>> UpdateStoreAsync(UpdateStoreDto request, string userId)
        {
            try
            {
                var store = await _context.Stores.FindAsync(request.Id);
                if (store == null)
                {
                    return ApiResponse<StoreDto>.ErrorResponse("Store not found");
                }

                // Check if name already exists (excluding current store)
                var duplicateName = await _context.Stores
                    .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower() && s.Id != request.Id);

                if (duplicateName)
                {
                    return ApiResponse<StoreDto>.ErrorResponse(
                        "A store with this name already exists");
                }

                _mapper.Map(request, store);
                store.UpdatedAt = DateTime.UtcNow;
                store.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Store updated",
                    new { StoreId = store.Id, Name = store.Name });

                var storeDto = _mapper.Map<StoreDto>(store);
                return ApiResponse<StoreDto>.SuccessResponse(
                    storeDto,
                    "Store updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating store");
                return ApiResponse<StoreDto>.ErrorResponse("An error occurred while updating store");
            }
        }

        public async Task<ApiResponse<bool>> DeleteStoreAsync(long id)
        {
            try
            {
                var store = await _context.Stores.FindAsync(id);
                if (store == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Store not found");
                }

                // Check if store has orders
                var hasOrders = await _context.Orders.AnyAsync(o => o.StoreId == id);
                if (hasOrders)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete store with existing orders",
                        new List<string> { "This store has order history and cannot be deleted." });
                }

                _context.Stores.Remove(store);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Store deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting store {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting store");
            }
        }

        public async Task<ApiResponse<bool>> UpdateCreditLimitAsync(UpdateCreditLimitDto request, string userId)
        {
            try
            {
                var store = await _context.Stores.FindAsync(request.StoreId);
                if (store == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Store not found");
                }

                var oldLimit = store.CreditLimit;
                store.CreditLimit = request.NewCreditLimit;
                store.UpdatedAt = DateTime.UtcNow;
                store.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Store credit limit updated",
                    new
                    {
                        StoreId = store.Id,
                        StoreName = store.Name,
                        OldLimit = oldLimit,
                        NewLimit = request.NewCreditLimit,
                        Reason = request.Reason
                    });

                return ApiResponse<bool>.SuccessResponse(true, "Credit limit updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating credit limit");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<StoreWithBalanceDto>> GetStoreWithBalanceAsync(long id)
        {
            try
            {
                var store = await _context.Stores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (store == null)
                {
                    return ApiResponse<StoreWithBalanceDto>.ErrorResponse("Store not found");
                }

                var storeDto = _mapper.Map<StoreWithBalanceDto>(store);

                // Calculate outstanding balance
                var invoices = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Payments)
                    .Where(i => i.Order.StoreId == id)
                    .AsNoTracking()
                    .ToListAsync();

                decimal totalOutstanding = 0;
                int overdueCount = 0;
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                foreach (var invoice in invoices)
                {
                    var totalPaid = invoice.Order.Payments.Sum(p => p.Amount);
                    var outstanding = invoice.TotalAmount - totalPaid;

                    if (outstanding > 0)
                    {
                        totalOutstanding += outstanding;

                        if (invoice.IssuedAt < thirtyDaysAgo)
                        {
                            overdueCount++;
                        }
                    }
                }

                storeDto.OutstandingBalance = totalOutstanding;
                storeDto.OverdueInvoiceCount = overdueCount;

                return ApiResponse<StoreWithBalanceDto>.SuccessResponse(storeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting store with balance");
                return ApiResponse<StoreWithBalanceDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<StoreWithBalanceDto>>> GetStoresWithOutstandingBalanceAsync()
        {
            try
            {
                var stores = await _context.Stores
                    .AsNoTracking()
                    .ToListAsync();

                var storesWithBalance = new List<StoreWithBalanceDto>();
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                foreach (var store in stores)
                {
                    var invoices = await _context.Invoices
                        .Include(i => i.Order)
                            .ThenInclude(o => o.Payments)
                        .Where(i => i.Order.StoreId == store.Id)
                        .AsNoTracking()
                        .ToListAsync();

                    decimal totalOutstanding = 0;
                    int overdueCount = 0;

                    foreach (var invoice in invoices)
                    {
                        var totalPaid = invoice.Order.Payments.Sum(p => p.Amount);
                        var outstanding = invoice.TotalAmount - totalPaid;

                        if (outstanding > 0)
                        {
                            totalOutstanding += outstanding;

                            if (invoice.IssuedAt < thirtyDaysAgo)
                            {
                                overdueCount++;
                            }
                        }
                    }

                    if (totalOutstanding > 0)
                    {
                        var dto = _mapper.Map<StoreWithBalanceDto>(store);
                        dto.OutstandingBalance = totalOutstanding;
                        dto.OverdueInvoiceCount = overdueCount;
                        storesWithBalance.Add(dto);
                    }
                }

                return ApiResponse<List<StoreWithBalanceDto>>.SuccessResponse(
                    storesWithBalance.OrderByDescending(s => s.OutstandingBalance).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stores with outstanding balance");
                return ApiResponse<List<StoreWithBalanceDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<LocationLookupDto>>> GetBarangaysAsync(string? city = null)
        {
            try
            {
                var query = _context.Stores
                    .AsNoTracking()
                    .Where(s => !string.IsNullOrEmpty(s.Barangay) && !string.IsNullOrEmpty(s.City));

                if (!string.IsNullOrWhiteSpace(city))
                {
                    query = query.Where(s => s.City == city);
                }

                var barangays = await query
                    .GroupBy(s => new { s.Barangay, s.City })
                    .Select(g => new LocationLookupDto
                    {
                        Barangay = g.Key.Barangay,
                        City = g.Key.City,
                        StoreCount = g.Count()
                    })
                    .OrderBy(l => l.City)
                    .ThenBy(l => l.Barangay)
                    .ToListAsync();

                return ApiResponse<List<LocationLookupDto>>.SuccessResponse(barangays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting barangays");
                return ApiResponse<List<LocationLookupDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<string>>> GetCitiesAsync()
        {
            try
            {
                var cities = await _context.Stores
                    .AsNoTracking()
                    .Where(s => !string.IsNullOrEmpty(s.City))
                    .Select(s => s.City)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResponse(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cities");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred");
            }
        }
    }
}
