using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ASTRASystem.Services
{
    public class DistributorService : IDistributorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<DistributorService> _logger;

        public DistributorService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            ILogger<DistributorService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<ApiResponse<DistributorDto>> GetDistributorByIdAsync(long id)
        {
            try
            {
                var distributor = await _context.Distributors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (distributor == null)
                {
                    return ApiResponse<DistributorDto>.ErrorResponse("Distributor not found");
                }

                var distributorDto = _mapper.Map<DistributorDto>(distributor);
                return ApiResponse<DistributorDto>.SuccessResponse(distributorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributor by ID {Id}", id);
                return ApiResponse<DistributorDto>.ErrorResponse("An error occurred while retrieving distributor");
            }
        }

        public async Task<ApiResponse<List<DistributorDto>>> GetDistributorsAsync()
        {
            try
            {
                var distributors = await _context.Distributors
                    .AsNoTracking()
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                var distributorDtos = _mapper.Map<List<DistributorDto>>(distributors);
                return ApiResponse<List<DistributorDto>>.SuccessResponse(distributorDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distributors");
                return ApiResponse<List<DistributorDto>>.ErrorResponse("An error occurred while retrieving distributors");
            }
        }

        public async Task<ApiResponse<DistributorDto>> CreateDistributorAsync(DistributorDto request, string userId)
        {
            try
            {
                // Check if name already exists
                var existingDistributor = await _context.Distributors
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == request.Name.ToLower());

                if (existingDistributor != null)
                {
                    return ApiResponse<DistributorDto>.ErrorResponse(
                        "A distributor with this name already exists");
                }

                var distributor = new Distributor
                {
                    Name = request.Name,
                    ContactPhone = request.ContactPhone,
                    Address = request.Address,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                _context.Distributors.Add(distributor);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Distributor created",
                    new { DistributorId = distributor.Id, Name = distributor.Name });

                var distributorDto = _mapper.Map<DistributorDto>(distributor);
                return ApiResponse<DistributorDto>.SuccessResponse(
                    distributorDto,
                    "Distributor created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating distributor");
                return ApiResponse<DistributorDto>.ErrorResponse("An error occurred while creating distributor");
            }
        }

        public async Task<ApiResponse<DistributorDto>> UpdateDistributorAsync(DistributorDto request, string userId)
        {
            try
            {
                var distributor = await _context.Distributors.FindAsync(request.Id);
                if (distributor == null)
                {
                    return ApiResponse<DistributorDto>.ErrorResponse("Distributor not found");
                }

                // Check if name already exists (excluding current distributor)
                var duplicateName = await _context.Distributors
                    .AnyAsync(d => d.Name.ToLower() == request.Name.ToLower() && d.Id != request.Id);

                if (duplicateName)
                {
                    return ApiResponse<DistributorDto>.ErrorResponse(
                        "A distributor with this name already exists");
                }

                distributor.Name = request.Name;
                distributor.ContactPhone = request.ContactPhone;
                distributor.Address = request.Address;
                distributor.UpdatedAt = DateTime.UtcNow;
                distributor.UpdatedById = userId;

                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Distributor updated",
                    new { DistributorId = distributor.Id, Name = distributor.Name });

                var distributorDto = _mapper.Map<DistributorDto>(distributor);
                return ApiResponse<DistributorDto>.SuccessResponse(
                    distributorDto,
                    "Distributor updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating distributor");
                return ApiResponse<DistributorDto>.ErrorResponse("An error occurred while updating distributor");
            }
        }

        public async Task<ApiResponse<bool>> DeleteDistributorAsync(long id)
        {
            try
            {
                var distributor = await _context.Distributors
                    .Include(d => d.Warehouses)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (distributor == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Distributor not found");
                }

                // Check if distributor has warehouses
                if (distributor.Warehouses.Any())
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete distributor with existing warehouses",
                        new List<string> { $"This distributor has {distributor.Warehouses.Count} warehouse(s). Please remove or reassign them first." });
                }

                // Check if distributor has associated orders
                var hasOrders = await _context.Orders.AnyAsync(o => o.DistributorId == id);
                if (hasOrders)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        "Cannot delete distributor with existing orders");
                }

                _context.Distributors.Remove(distributor);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Distributor deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting distributor {Id}", id);
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting distributor");
            }
        }
    }
}
