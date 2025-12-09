using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Location;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Services
{
    public class CityService : ICityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<CityService> _logger;

        public CityService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<CityService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ApiResponse<CityDto>> GetCityByIdAsync(long id)
        {
            try
            {
                var city = await _context.Cities
                    .Include(c => c.Barangays)
                    .Include(c => c.Stores)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (city == null)
                {
                    return ApiResponse<CityDto>.ErrorResponse("City not found");
                }

                var cityDto = _mapper.Map<CityDto>(city);
                return ApiResponse<CityDto>.SuccessResponse(cityDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting city by ID {Id}", id);
                return ApiResponse<CityDto>.ErrorResponse("An error occurred while retrieving city");
            }
        }

        public async Task<ApiResponse<CityDto>> GetCityByNameAsync(string name)
        {
            try
            {
                var city = await _context.Cities
                    .Include(c => c.Barangays)
                    .Include(c => c.Stores)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

                if (city == null)
                {
                    return ApiResponse<CityDto>.ErrorResponse("City not found");
                }

                var cityDto = _mapper.Map<CityDto>(city);
                return ApiResponse<CityDto>.SuccessResponse(cityDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting city by name {Name}", name);
                return ApiResponse<CityDto>.ErrorResponse("An error occurred while retrieving city");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<CityDto>>> GetCitiesAsync(CityQueryDto query)
        {
            try
            {
                var citiesQuery = _context.Cities
                    .Include(c => c.Barangays)
                    .Include(c => c.Stores)
                    .AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchLower = query.SearchTerm.ToLower();
                    citiesQuery = citiesQuery.Where(c =>
                        c.Name.ToLower().Contains(searchLower) ||
                        (c.Province != null && c.Province.ToLower().Contains(searchLower)) ||
                        (c.Region != null && c.Region.ToLower().Contains(searchLower)));
                }

                if (!string.IsNullOrWhiteSpace(query.Province))
                {
                    citiesQuery = citiesQuery.Where(c => c.Province == query.Province);
                }

                if (!string.IsNullOrWhiteSpace(query.Region))
                {
                    citiesQuery = citiesQuery.Where(c => c.Region == query.Region);
                }

                if (query.IsActive.HasValue)
                {
                    citiesQuery = citiesQuery.Where(c => c.IsActive == query.IsActive.Value);
                }

                // Apply sorting
                citiesQuery = query.SortBy.ToLower() switch
                {
                    "name" => query.SortDescending
                        ? citiesQuery.OrderByDescending(c => c.Name)
                        : citiesQuery.OrderBy(c => c.Name),
                    "province" => query.SortDescending
                        ? citiesQuery.OrderByDescending(c => c.Province)
                        : citiesQuery.OrderBy(c => c.Province),
                    "region" => query.SortDescending
                        ? citiesQuery.OrderByDescending(c => c.Region)
                        : citiesQuery.OrderBy(c => c.Region),
                    "barangaycount" => query.SortDescending
                        ? citiesQuery.OrderByDescending(c => c.Barangays.Count)
                        : citiesQuery.OrderBy(c => c.Barangays.Count),
                    _ => citiesQuery.OrderBy(c => c.Name)
                };

                var totalCount = await citiesQuery.CountAsync();
                var cities = await citiesQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var cityDtos = _mapper.Map<List<CityDto>>(cities);
                var paginatedResponse = new PaginatedResponse<CityDto>(
                    cityDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<CityDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cities");
                return ApiResponse<PaginatedResponse<CityDto>>.ErrorResponse("An error occurred while retrieving cities");
            }
        }

        public async Task<ApiResponse<List<CityListItemDto>>> GetCitiesForLookupAsync(string? searchTerm = null)
        {
            try
            {
                var query = _context.Cities
                    .Include(c => c.Barangays)
                    .Include(c => c.Stores)
                    .Where(c => c.IsActive)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(c =>
                        c.Name.ToLower().Contains(searchLower) ||
                        (c.Province != null && c.Province.ToLower().Contains(searchLower)));
                }

                var cities = await query
                    .OrderBy(c => c.Name)
                    .Take(100)
                    .ToListAsync();

                var cityDtos = _mapper.Map<List<CityListItemDto>>(cities);
                return ApiResponse<List<CityListItemDto>>.SuccessResponse(cityDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cities for lookup");
                return ApiResponse<List<CityListItemDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<CityDto>> CreateCityAsync(CreateCityDto request, string userId)
        {
            try
            {
                // Check if city name already exists in the same province
                var existingCity = await _context.Cities
                    .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower() &&
                                 c.Province == request.Province);

                if (existingCity)
                {
                    return ApiResponse<CityDto>.ErrorResponse(
                        "A city with this name already exists in this province");
                }

                var city = _mapper.Map<City>(request);
                city.CreatedAt = DateTime.UtcNow;
                city.UpdatedAt = DateTime.UtcNow;
                city.CreatedById = userId;
                city.UpdatedById = userId;

                _context.Cities.Add(city);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "City created",
                    new { CityId = city.Id, Name = city.Name, Province = city.Province });

                // Reload with includes for proper mapping
                city = await _context.Cities
                    .Include(c => c.Barangays)
                    .Include(c => c.Stores)
                    .FirstAsync(c => c.Id == city.Id);

                var cityDto = _mapper.Map<CityDto>(city);
                return ApiResponse<CityDto>.SuccessResponse(
                    cityDto,
                    "City created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating city");
                return ApiResponse<CityDto>.ErrorResponse("An error occurred while creating city");
            }
        }

        public async Task<ApiResponse<CityDto>> UpdateCityAsync(UpdateCityDto request, string userId)
        {
            try
            {
                var city = await _context.Cities
                    .Include(c => c.Barangays)
                    .Include(c => c.Stores)
                    .FirstOrDefaultAsync(c => c.Id == request.Id);

                if (city == null)
                {
                    return ApiResponse<CityDto>.ErrorResponse("City not found");
                }

                // Check if name already exists (excluding current city)
                var duplicateName = await _context.Cities
                    .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower() &&
                                 c.Province == request.Province &&
                                 c.Id != request.Id);

                if (duplicateName)
                {
                    return ApiResponse<CityDto>.ErrorResponse(
                        "A city with this name already exists in this province");
                }

                _mapper.Map(request, city);
                city.UpdatedAt = DateTime.UtcNow;
                city.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "City updated",
                    new { CityId = city.Id, Name = city.Name });

                var cityDto = _mapper.Map<CityDto>(city);
                return ApiResponse<CityDto>.SuccessResponse(
                    cityDto,
                    "City updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city");
                return ApiResponse<CityDto>.ErrorResponse("An error occurred while updating city");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCityAsync(long id)
        {
            try
            {
                var city = await _context.Cities
                    .Include(c => c.Barangays)
                    .Include(c => c.Stores)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (city == null)
                {
                    return ApiResponse<bool>.ErrorResponse("City not found");
                }

                // Check if city has barangays
                if (city.Barangays.Any())
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete city with existing barangays",
                        new List<string> { "Please delete all barangays first." });
                }

                // Check if city has stores
                if (city.Stores.Any())
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete city with existing stores",
                        new List<string> { "This city has stores and cannot be deleted." });
                }

                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "City deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting city");
            }
        }

        public async Task<ApiResponse<List<string>>> GetProvincesAsync()
        {
            try
            {
                var provinces = await _context.Cities
                    .AsNoTracking()
                    .Where(c => !string.IsNullOrEmpty(c.Province))
                    .Select(c => c.Province)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResponse(provinces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provinces");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<string>>> GetRegionsAsync()
        {
            try
            {
                var regions = await _context.Cities
                    .AsNoTracking()
                    .Where(c => !string.IsNullOrEmpty(c.Region))
                    .Select(c => c.Region)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToListAsync();

                return ApiResponse<List<string>>.SuccessResponse(regions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting regions");
                return ApiResponse<List<string>>.ErrorResponse("An error occurred");
            }
        }
    }
}
