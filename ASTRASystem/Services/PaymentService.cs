
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Payment;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace ASTRASystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            ApplicationDbContext context,
            IMapper mapper,
            IAuditLogService auditLogService,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(long id)
        {
            try
            {
                var payment = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResponse("Payment not found");
                }

                var paymentDto = _mapper.Map<PaymentDto>(payment);

                if (!string.IsNullOrEmpty(payment.RecordedById))
                {
                    var user = await _userManager.FindByIdAsync(payment.RecordedById);
                    paymentDto.RecordedByName = user?.FullName;
                }

                return ApiResponse<PaymentDto>.SuccessResponse(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by ID {Id}", id);
                return ApiResponse<PaymentDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<PaymentDto>>> GetPaymentsAsync(PaymentQueryDto query)
        {
            try
            {
                var paymentsQuery = _context.Payments.AsNoTracking();

                // Apply filters
                if (query.OrderId.HasValue)
                {
                    paymentsQuery = paymentsQuery.Where(p => p.OrderId == query.OrderId.Value);
                }

                if (query.StoreId.HasValue)
                {
                    paymentsQuery = paymentsQuery.Where(p => p.Order.StoreId == query.StoreId.Value);
                }

                if (query.Method.HasValue)
                {
                    paymentsQuery = paymentsQuery.Where(p => p.Method == query.Method.Value);
                }

                if (query.RecordedFrom.HasValue)
                {
                    paymentsQuery = paymentsQuery.Where(p => p.RecordedAt >= query.RecordedFrom.Value);
                }

                if (query.RecordedTo.HasValue)
                {
                    paymentsQuery = paymentsQuery.Where(p => p.RecordedAt <= query.RecordedTo.Value);
                }

                // Apply sorting
                paymentsQuery = query.SortBy.ToLower() switch
                {
                    "amount" => query.SortDescending
                        ? paymentsQuery.OrderByDescending(p => p.Amount)
                        : paymentsQuery.OrderBy(p => p.Amount),
                    _ => query.SortDescending
                        ? paymentsQuery.OrderByDescending(p => p.RecordedAt)
                        : paymentsQuery.OrderBy(p => p.RecordedAt)
                };

                var totalCount = await paymentsQuery.CountAsync();
                var payments = await paymentsQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var paymentDtos = new List<PaymentDto>();
                foreach (var payment in payments)
                {
                    var dto = _mapper.Map<PaymentDto>(payment);

                    if (!string.IsNullOrEmpty(payment.RecordedById))
                    {
                        var user = await _userManager.FindByIdAsync(payment.RecordedById);
                        dto.RecordedByName = user?.FullName;
                    }

                    paymentDtos.Add(dto);
                }

                var paginatedResponse = new PaginatedResponse<PaymentDto>(
                    paymentDtos, totalCount, query.PageNumber, query.PageSize);

                return ApiResponse<PaginatedResponse<PaymentDto>>.SuccessResponse(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return ApiResponse<PaginatedResponse<PaymentDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<PaymentDto>> RecordPaymentAsync(RecordPaymentDto request, string userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return ApiResponse<PaymentDto>.ErrorResponse("Order not found");
                }

                // Calculate remaining balance
                var totalPaid = order.Payments.Sum(p => p.Amount);
                var remainingBalance = order.Total - totalPaid;

                if (request.Amount > remainingBalance)
                {
                    return ApiResponse<PaymentDto>.ErrorResponse(
                        $"Payment amount exceeds remaining balance of {remainingBalance:C}");
                }

                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Method = request.Method,
                    Reference = request.Reference,
                    RecordedById = userId,
                    RecordedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await _auditLogService.LogActionAsync(
                    userId,
                    "Payment recorded",
                    new
                    {
                        PaymentId = payment.Id,
                        OrderId = request.OrderId,
                        Amount = request.Amount,
                        Method = request.Method.ToString()
                    });

                var paymentDto = _mapper.Map<PaymentDto>(payment);
                var user = await _userManager.FindByIdAsync(userId);
                paymentDto.RecordedByName = user?.FullName;

                return ApiResponse<PaymentDto>.SuccessResponse(
                    paymentDto,
                    "Payment recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment");
                return ApiResponse<PaymentDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<PaymentDto>>> GetPaymentsByOrderAsync(long orderId)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.OrderId == orderId)
                    .AsNoTracking()
                    .OrderBy(p => p.RecordedAt)
                    .ToListAsync();

                var paymentDtos = new List<PaymentDto>();
                foreach (var payment in payments)
                {
                    var dto = _mapper.Map<PaymentDto>(payment);

                    if (!string.IsNullOrEmpty(payment.RecordedById))
                    {
                        var user = await _userManager.FindByIdAsync(payment.RecordedById);
                        dto.RecordedByName = user?.FullName;
                    }

                    paymentDtos.Add(dto);
                }

                return ApiResponse<List<PaymentDto>>.SuccessResponse(paymentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for order");
                return ApiResponse<List<PaymentDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ReconcilePaymentAsync(ReconcilePaymentDto request, string userId)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(request.PaymentId);
                if (payment == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Payment not found");
                }

                // In a real system, you'd have a reconciliation status field
                // For now, we'll just log the reconciliation
                await _auditLogService.LogActionAsync(
                    userId,
                    "Payment reconciled",
                    new { PaymentId = payment.Id, Notes = request.Notes });

                return ApiResponse<bool>.SuccessResponse(true, "Payment reconciled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling payment");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<CashCollectionSummaryDto>> GetCashCollectionSummaryAsync(
            long? tripId = null,
            string? dispatcherId = null,
            DateTime? date = null)
        {
            try
            {
                var paymentsQuery = _context.Payments.AsNoTracking();

                // Filter by trip (if orders are part of a trip)
                if (tripId.HasValue)
                {
                    var tripOrderIds = await _context.TripAssignments
                        .Where(ta => ta.TripId == tripId.Value)
                        .Select(ta => ta.OrderId)
                        .ToListAsync();

                    paymentsQuery = paymentsQuery.Where(p => tripOrderIds.Contains(p.OrderId));
                }

                // Filter by date
                if (date.HasValue)
                {
                    var startOfDay = date.Value.Date;
                    var endOfDay = startOfDay.AddDays(1);
                    paymentsQuery = paymentsQuery.Where(p => p.RecordedAt >= startOfDay && p.RecordedAt < endOfDay);
                }

                var payments = await paymentsQuery.ToListAsync();

                var summary = new CashCollectionSummaryDto
                {
                    TripId = tripId,
                    DispatcherId = dispatcherId,
                    CollectionDate = date ?? DateTime.Today,
                    TotalCash = payments.Where(p => p.Method == Enum.PaymentMethod.Cash).Sum(p => p.Amount),
                    TotalGCash = payments.Where(p => p.Method == Enum.PaymentMethod.GCash).Sum(p => p.Amount),
                    TotalMaya = payments.Where(p => p.Method == Enum.PaymentMethod.Maya).Sum(p => p.Amount),
                    TotalBankTransfer = payments.Where(p => p.Method == Enum.PaymentMethod.BankTransfer).Sum(p => p.Amount),
                    TotalOther = payments.Where(p => p.Method == Enum.PaymentMethod.Other).Sum(p => p.Amount),
                    PaymentCount = payments.Count
                };

                summary.GrandTotal = summary.TotalCash + summary.TotalGCash + summary.TotalMaya +
                                    summary.TotalBankTransfer + summary.TotalOther;

                return ApiResponse<CashCollectionSummaryDto>.SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cash collection summary");
                return ApiResponse<CashCollectionSummaryDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<PaymentReconciliationDto>>> GetUnreconciledPaymentsAsync()
        {
            try
            {
                // In a real system, you'd have a reconciliation status field
                // For this implementation, we'll return all payments from the last 30 days
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var payments = await _context.Payments
                    .Where(p => p.RecordedAt >= thirtyDaysAgo)
                    .AsNoTracking()
                    .OrderByDescending(p => p.RecordedAt)
                    .ToListAsync();

                var dtos = payments.Select(p => new PaymentReconciliationDto
                {
                    PaymentId = p.Id,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    Method = p.Method,
                    Reference = p.Reference,
                    RecordedAt = p.RecordedAt,
                    IsReconciled = false,
                    ReconciledAt = null,
                    ReconciledById = null
                }).ToList();

                return ApiResponse<List<PaymentReconciliationDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unreconciled payments");
                return ApiResponse<List<PaymentReconciliationDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<decimal>> GetOrderBalanceAsync(long orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Payments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse<decimal>.ErrorResponse("Order not found");
                }

                var totalPaid = order.Payments.Sum(p => p.Amount);
                var balance = order.Total - totalPaid;

                return ApiResponse<decimal>.SuccessResponse(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order balance");
                return ApiResponse<decimal>.ErrorResponse("An error occurred");
            }
        }
    }
}
