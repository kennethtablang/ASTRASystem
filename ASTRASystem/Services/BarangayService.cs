using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Location;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Services
{
    public class BarangayService : IBarangayService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<BarangayService> _logger;

        public BarangayService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<BarangayService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ApiResponse<BarangayDto>> GetBarangayByIdAsync(long id)
        {
            try
            {
                var barangay = await _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (barangay == null)
                {
                    return ApiResponse<BarangayDto>.ErrorResponse("Barangay not found");
                }

                var barangayDto = _mapper.Map<BarangayDto>(barangay);
                return ApiResponse<BarangayDto>.SuccessResponse(barangayDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting barangay by ID {Id}", id);
                return ApiResponse<BarangayDto>.ErrorResponse("An error occurred while retrieving barangay");
            }
        }

        public async Task<ApiResponse<BarangayDto>> GetBarangayByNameAsync(string name, long cityId)
        {
            try
            {
                var barangay = await _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower() && b.CityId == cityId);

                if (barangay == null)
                {
                    return ApiResponse<BarangayDto>.ErrorResponse("Barangay not found");
                }

                var barangayDto = _mapper.Map<BarangayDto>(barangay);
                return ApiResponse<BarangayDto>.SuccessResponse(barangayDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting barangay by name {Name}", name);
                return ApiResponse<BarangayDto>.ErrorResponse("An error occurred while retrieving barangay");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<BarangayDto>>> GetBarangaysAsync(BarangayQueryDto query)
        {
            try
            {
                var barangaysQuery = _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchLower = query.SearchTerm.ToLower();
                    barangaysQuery = barangaysQuery.Where(b =>
                        b.Name.ToLower().Contains(searchLower) ||
                        b.City.Name.ToLower().Contains(searchLower) ||
                        (b.ZipCode != null && b.ZipCode.Contains(query.SearchTerm)));
                }

                if (query.CityId.HasValue)
                {
                    barangaysQuery = barangaysQuery.Where(b => b.CityId == query.CityId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.ZipCode))
                {
                    barangaysQuery = barangaysQuery.Where(b => b.ZipCode == query.ZipCode);
                }

                if (query.IsActive.HasValue)
                {
                    barangaysQuery = barangaysQuery.Where(b => b.IsActive == query.IsActive.Value);
                }

                // Apply sorting
                barangaysQuery = query.SortBy.ToLower() switch
                {
                    "name" => query.SortDescending
                        ? barangaysQuery.OrderByDescending(b => b.Name)
                        : barangaysQuery.OrderBy(b => b.Name),
                    "city" => query.SortDescending
                        ? barangaysQuery.OrderByDescending(b => b.City.Name)
                        : barangaysQuery.OrderBy(b => b.City.Name),
                    "zipcode" => query.SortDescending
                        ? barangaysQuery.OrderByDescending(b => b.ZipCode)
                        : barangaysQuery.OrderBy(b => b.ZipCode),
                    "storecount" => query.SortDescending
                        ? barangaysQuery.OrderByDescending(b => b.Stores.Count)
                        : barangaysQuery.OrderBy(b => b.Stores.Count),
                    _ => barangaysQuery.OrderBy(b => b.City.Name).ThenBy(b => b.Name)
                };

                var totalCount = await barangaysQuery.CountAsync();
                var barangays = await barangaysQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var barangayDtos = _mapper.Map<List<BarangayDto>>(barangays);
                var paginatedResponse = new PaginatedResponse<BarangayDto>(
                    barangayDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<BarangayDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting barangays");
                return ApiResponse<PaginatedResponse<BarangayDto>>.ErrorResponse("An error occurred while retrieving barangays");
            }
        }

        public async Task<ApiResponse<List<BarangayListItemDto>>> GetBarangaysByCityAsync(long cityId)
        {
            try
            {
                var barangays = await _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .Where(b => b.CityId == cityId && b.IsActive)
                    .OrderBy(b => b.Name)
                    .AsNoTracking()
                    .ToListAsync();

                var barangayDtos = _mapper.Map<List<BarangayListItemDto>>(barangays);
                return ApiResponse<List<BarangayListItemDto>>.SuccessResponse(barangayDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting barangays by city {CityId}", cityId);
                return ApiResponse<List<BarangayListItemDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<BarangayListItemDto>>> GetBarangaysForLookupAsync(long? cityId = null, string? searchTerm = null)
        {
            try
            {
                var query = _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .Where(b => b.IsActive)
                    .AsNoTracking();

                if (cityId.HasValue)
                {
                    query = query.Where(b => b.CityId == cityId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(b =>
                        b.Name.ToLower().Contains(searchLower) ||
                        b.City.Name.ToLower().Contains(searchLower));
                }

                var barangays = await query
                    .OrderBy(b => b.City.Name)
                    .ThenBy(b => b.Name)
                    .Take(100)
                    .ToListAsync();

                var barangayDtos = _mapper.Map<List<BarangayListItemDto>>(barangays);
                return ApiResponse<List<BarangayListItemDto>>.SuccessResponse(barangayDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting barangays for lookup");
                return ApiResponse<List<BarangayListItemDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<BarangayDto>> CreateBarangayAsync(CreateBarangayDto request, string userId)
        {
            try
            {
                // Verify city exists
                var cityExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId);
                if (!cityExists)
                {
                    return ApiResponse<BarangayDto>.ErrorResponse("City not found");
                }

                // Check if barangay name already exists in the same city
                var existingBarangay = await _context.Barangays
                    .AnyAsync(b => b.Name.ToLower() == request.Name.ToLower() &&
                                 b.CityId == request.CityId);

                if (existingBarangay)
                {
                    return ApiResponse<BarangayDto>.ErrorResponse(
                        "A barangay with this name already exists in this city");
                }

                var barangay = _mapper.Map<Barangay>(request);
                barangay.CreatedAt = DateTime.UtcNow;
                barangay.UpdatedAt = DateTime.UtcNow;
                barangay.CreatedById = userId;
                barangay.UpdatedById = userId;

                _context.Barangays.Add(barangay);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Barangay created",
                    new { BarangayId = barangay.Id, Name = barangay.Name, CityId = barangay.CityId });

                // Reload with includes for proper mapping
                barangay = await _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .FirstAsync(b => b.Id == barangay.Id);

                var barangayDto = _mapper.Map<BarangayDto>(barangay);
                return ApiResponse<BarangayDto>.SuccessResponse(
                    barangayDto,
                    "Barangay created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating barangay");
                return ApiResponse<BarangayDto>.ErrorResponse("An error occurred while creating barangay");
            }
        }

        public async Task<ApiResponse<List<BarangayDto>>> BulkCreateBarangaysAsync(BulkCreateBarangaysDto request, string userId)
        {
            try
            {
                // Verify city exists
                var city = await _context.Cities
                    .Include(c => c.Barangays)
                    .FirstOrDefaultAsync(c => c.Id == request.CityId);

                if (city == null)
                {
                    return ApiResponse<List<BarangayDto>>.ErrorResponse("City not found");
                }

                var createdBarangays = new List<Barangay>();
                var existingNames = new List<string>();

                foreach (var barangayName in request.BarangayNames)
                {
                    // Check if already exists
                    var exists = await _context.Barangays
                        .AnyAsync(b => b.Name.ToLower() == barangayName.ToLower() &&
                                     b.CityId == request.CityId);

                    if (exists)
                    {
                        existingNames.Add(barangayName);
                        continue;
                    }

                    var barangay = new Barangay
                    {
                        Name = barangayName,
                        CityId = request.CityId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = userId,
                        UpdatedById = userId
                    };

                    createdBarangays.Add(barangay);
                }

                if (createdBarangays.Any())
                {
                    _context.Barangays.AddRange(createdBarangays);
                    await _context.SaveChangesAsync();

                    await _auditLogService.LogActionAsync(
                        userId,
                        "Bulk barangays created",
                        new
                        {
                            CityId = request.CityId,
                            CityName = city.Name,
                            Count = createdBarangays.Count
                        });

                    // Reload with includes
                    var barangayIds = createdBarangays.Select(b => b.Id).ToList();
                    var reloadedBarangays = await _context.Barangays
                        .Include(b => b.City)
                        .Include(b => b.Stores)
                        .Where(b => barangayIds.Contains(b.Id))
                        .ToListAsync();

                    var barangayDtos = _mapper.Map<List<BarangayDto>>(reloadedBarangays);

                    var message = existingNames.Any()
                        ? $"{createdBarangays.Count} barangays created successfully. Skipped {existingNames.Count} duplicates: {string.Join(", ", existingNames)}"
                        : $"{createdBarangays.Count} barangays created successfully";

                    return ApiResponse<List<BarangayDto>>.SuccessResponse(barangayDtos, message);
                }

                return ApiResponse<List<BarangayDto>>.ErrorResponse(
                    "All barangays already exist",
                    new List<string> { $"Existing: {string.Join(", ", existingNames)}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk creating barangays");
                return ApiResponse<List<BarangayDto>>.ErrorResponse("An error occurred while creating barangays");
            }
        }

        public async Task<ApiResponse<BarangayDto>> UpdateBarangayAsync(UpdateBarangayDto request, string userId)
        {
            try
            {
                var barangay = await _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .FirstOrDefaultAsync(b => b.Id == request.Id);

                if (barangay == null)
                {
                    return ApiResponse<BarangayDto>.ErrorResponse("Barangay not found");
                }

                // Verify city exists if changed
                if (barangay.CityId != request.CityId)
                {
                    var cityExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId);
                    if (!cityExists)
                    {
                        return ApiResponse<BarangayDto>.ErrorResponse("City not found");
                    }
                }

                // Check if name already exists (excluding current barangay)
                var duplicateName = await _context.Barangays
                    .AnyAsync(b => b.Name.ToLower() == request.Name.ToLower() &&
                                 b.CityId == request.CityId &&
                                 b.Id != request.Id);

                if (duplicateName)
                {
                    return ApiResponse<BarangayDto>.ErrorResponse(
                        "A barangay with this name already exists in this city");
                }

                _mapper.Map(request, barangay);
                barangay.UpdatedAt = DateTime.UtcNow;
                barangay.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Barangay updated",
                    new { BarangayId = barangay.Id, Name = barangay.Name });

                // Reload to get updated city info
                barangay = await _context.Barangays
                    .Include(b => b.City)
                    .Include(b => b.Stores)
                    .FirstAsync(b => b.Id == barangay.Id);

                var barangayDto = _mapper.Map<BarangayDto>(barangay);
                return ApiResponse<BarangayDto>.SuccessResponse(
                    barangayDto,
                    "Barangay updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating barangay");
                return ApiResponse<BarangayDto>.ErrorResponse("An error occurred while updating barangay");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBarangayAsync(long id)
        {
            try
            {
                var barangay = await _context.Barangays
                    .Include(b => b.Stores)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (barangay == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Barangay not found");
                }

                // Check if barangay has stores
                if (barangay.Stores.Any())
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete barangay with existing stores",
                        new List<string> { "This barangay has stores and cannot be deleted." });
                }

                _context.Barangays.Remove(barangay);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Barangay deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting barangay {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting barangay");
            }
        }
    }
}
