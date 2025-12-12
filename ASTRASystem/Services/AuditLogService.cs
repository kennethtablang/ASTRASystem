using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ASTRASystem.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<AuditLogService> logger)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task LogActionAsync(string userId, string action, object? metadata = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Meta = metadata != null ? JsonSerializer.Serialize(metadata) : string.Empty,
                    OccurredAt = DateTime.UtcNow,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action for user {UserId}", userId);
            }
        }

        public async Task<ApiResponse<PaginatedResponse<AuditLogDto>>> GetAuditLogsAsync(AuditLogQueryDto query)
        {
            try
            {
                var logsQuery = _context.AuditLogs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query.UserId))
                {
                    logsQuery = logsQuery.Where(a => a.UserId == query.UserId);
                }

                if (!string.IsNullOrWhiteSpace(query.Action))
                {
                    logsQuery = logsQuery.Where(a => a.Action.Contains(query.Action));
                }

                if (query.From.HasValue)
                {
                    logsQuery = logsQuery.Where(a => a.OccurredAt >= query.From.Value);
                }

                if (query.To.HasValue)
                {
                    logsQuery = logsQuery.Where(a => a.OccurredAt <= query.To.Value);
                }

                var totalCount = await logsQuery.CountAsync();
                var logs = await logsQuery
                    .OrderByDescending(a => a.OccurredAt)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var logDtos = new List<AuditLogDto>();
                foreach (var log in logs)
                {
                    var dto = _mapper.Map<AuditLogDto>(log);
                    var user = await _userManager.FindByIdAsync(log.UserId);
                    dto.UserName = user?.FullName ?? "Unknown";
                    logDtos.Add(dto);
                }

                var paginatedResponse = new PaginatedResponse<AuditLogDto>(
                    logDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<AuditLogDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs");
                return ApiResponse<PaginatedResponse<AuditLogDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<AuditLogDto>>> GetUserAuditLogsAsync(string userId, int limit = 50)
        {
            try
            {
                var logs = await _context.AuditLogs
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.OccurredAt)
                    .Take(limit)
                    .ToListAsync();

                var logDtos = logs.Select(log =>
                {
                    var dto = _mapper.Map<AuditLogDto>(log);
                    dto.UserName = userId;
                    return dto;
                }).ToList();

                return ApiResponse<List<AuditLogDto>>.SuccessResponse(logDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user audit logs");
                return ApiResponse<List<AuditLogDto>>.ErrorResponse("An error occurred");
            }
        }
    }
}
