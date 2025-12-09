using Microsoft.EntityFrameworkCore;
using ASTRASystem.Data;
using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Payment;
using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using AutoMapper;

namespace ASTRASystem.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPdfService _pdfService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            ApplicationDbContext context,
            IMapper mapper,
            IPdfService pdfService,
            IFileStorageService fileStorageService,
            ILogger<InvoiceService> logger)
        {
            _context = context;
            _mapper = mapper;
            _pdfService = pdfService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<InvoiceDto>> GetInvoiceByIdAsync(long id)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.Barangay)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.City)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    return ApiResponse<InvoiceDto>.ErrorResponse("Invoice not found");
                }

                var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
                return ApiResponse<InvoiceDto>.SuccessResponse(invoiceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice by ID {Id}", id);
                return ApiResponse<InvoiceDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<InvoiceDto>> GetInvoiceByOrderIdAsync(long orderId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.Barangay)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.City)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.OrderId == orderId);

                if (invoice == null)
                {
                    return ApiResponse<InvoiceDto>.ErrorResponse("Invoice not found");
                }

                var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
                return ApiResponse<InvoiceDto>.SuccessResponse(invoiceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice by order ID");
                return ApiResponse<InvoiceDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<InvoiceDto>> GenerateInvoiceAsync(GenerateInvoiceDto request, string userId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Store)
                        .ThenInclude(s => s.Barangay)
                    .Include(o => o.Store)
                        .ThenInclude(s => s.City)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return ApiResponse<InvoiceDto>.ErrorResponse("Order not found");
                }

                // Check if invoice already exists
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.OrderId == request.OrderId);

                if (existingInvoice != null)
                {
                    return ApiResponse<InvoiceDto>.ErrorResponse("Invoice already exists for this order");
                }

                var invoice = new Invoice
                {
                    OrderId = request.OrderId,
                    TotalAmount = order.Total,
                    TaxAmount = order.Tax,
                    IssuedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                    UpdatedById = userId
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Generate PDF
                var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

                // Upload PDF to storage
                var fileName = $"invoice_{invoice.Id}_{DateTime.UtcNow:yyyyMMdd}.pdf";
                var fileUrl = await _fileStorageService.UploadFileAsync(pdfBytes, fileName, "application/pdf");

                invoice.InvoiceUrl = fileUrl;
                await _context.SaveChangesAsync();

                // Reload with relationships
                invoice = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.Barangay)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.City)
                    .FirstAsync(i => i.Id == invoice.Id);

                var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
                return ApiResponse<InvoiceDto>.SuccessResponse(
                    invoiceDto,
                    "Invoice generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice");
                return ApiResponse<InvoiceDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(long invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.Barangay)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.City)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Items)
                            .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    return ApiResponse<byte[]>.ErrorResponse("Invoice not found");
                }

                var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
                return ApiResponse<byte[]>.SuccessResponse(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice PDF");
                return ApiResponse<byte[]>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<AccountsReceivableSummaryDto>> GetARSummaryAsync()
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Payments)
                    .AsNoTracking()
                    .ToListAsync();

                var summary = new AccountsReceivableSummaryDto
                {
                    TotalInvoices = invoices.Count
                };

                foreach (var invoice in invoices)
                {
                    var totalPaid = invoice.Order.Payments.Sum(p => p.Amount);
                    var outstanding = invoice.TotalAmount - totalPaid;

                    if (outstanding > 0)
                    {
                        summary.TotalOutstanding += outstanding;

                        var daysOld = (DateTime.UtcNow - invoice.IssuedAt).Days;

                        if (daysOld <= 30)
                            summary.Current += outstanding;
                        else if (daysOld <= 60)
                            summary.Aging30 += outstanding;
                        else if (daysOld <= 90)
                            summary.Aging60 += outstanding;
                        else
                            summary.Aging90Plus += outstanding;

                        if (daysOld > 30)
                            summary.OverdueInvoices++;
                    }
                }

                return ApiResponse<AccountsReceivableSummaryDto>.SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AR summary");
                return ApiResponse<AccountsReceivableSummaryDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<ARAgingLineDto>>> GetARAgingReportAsync()
        {
            try
            {
                var storeInvoices = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.Barangay)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Payments)
                    .AsNoTracking()
                    .GroupBy(i => i.Order.StoreId)
                    .ToListAsync();

                var agingLines = new List<ARAgingLineDto>();

                foreach (var group in storeInvoices)
                {
                    var store = await _context.Stores
                        .Include(s => s.Barangay)
                        .FirstOrDefaultAsync(s => s.Id == group.Key);

                    if (store == null) continue;

                    var line = new ARAgingLineDto
                    {
                        StoreId = store.Id,
                        StoreName = store.Name,
                        StoreBarangay = store.Barangay?.Name,
                        CreditLimit = store.CreditLimit,
                        InvoiceCount = group.Count()
                    };

                    foreach (var invoice in group)
                    {
                        var totalPaid = invoice.Order.Payments.Sum(p => p.Amount);
                        var outstanding = invoice.TotalAmount - totalPaid;

                        if (outstanding > 0)
                        {
                            line.TotalOutstanding += outstanding;

                            var daysOld = (DateTime.UtcNow - invoice.IssuedAt).Days;

                            if (daysOld <= 30)
                                line.Current += outstanding;
                            else if (daysOld <= 60)
                                line.Aging30 += outstanding;
                            else if (daysOld <= 90)
                                line.Aging60 += outstanding;
                            else
                                line.Aging90Plus += outstanding;
                        }
                    }

                    if (line.TotalOutstanding > 0)
                    {
                        agingLines.Add(line);
                    }
                }

                return ApiResponse<List<ARAgingLineDto>>.SuccessResponse(
                    agingLines.OrderByDescending(l => l.TotalOutstanding).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AR aging report");
                return ApiResponse<List<ARAgingLineDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<InvoiceDto>>> GetOverdueInvoicesAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var overdueInvoices = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.Barangay)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.City)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Payments)
                    .Where(i => i.IssuedAt < thirtyDaysAgo)
                    .AsNoTracking()
                    .ToListAsync();

                var overdueDtos = new List<InvoiceDto>();

                foreach (var invoice in overdueInvoices)
                {
                    var totalPaid = invoice.Order.Payments.Sum(p => p.Amount);
                    if (totalPaid < invoice.TotalAmount)
                    {
                        overdueDtos.Add(_mapper.Map<InvoiceDto>(invoice));
                    }
                }

                return ApiResponse<List<InvoiceDto>>.SuccessResponse(overdueDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue invoices");
                return ApiResponse<List<InvoiceDto>>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<List<InvoiceDto>>> GetInvoicesByStoreAsync(long storeId)
        {
            try
            {
                var invoices = await _context.Invoices
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.Barangay)
                    .Include(i => i.Order)
                        .ThenInclude(o => o.Store)
                            .ThenInclude(s => s.City)
                    .Where(i => i.Order.StoreId == storeId)
                    .AsNoTracking()
                    .OrderByDescending(i => i.IssuedAt)
                    .ToListAsync();

                var invoiceDtos = _mapper.Map<List<InvoiceDto>>(invoices);
                return ApiResponse<List<InvoiceDto>>.SuccessResponse(invoiceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by store");
                return ApiResponse<List<InvoiceDto>>.ErrorResponse("An error occurred");
            }
        }
    }
}