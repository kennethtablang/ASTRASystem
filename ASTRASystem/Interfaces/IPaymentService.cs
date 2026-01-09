using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Payment;

namespace ASTRASystem.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(long id);
        Task<ApiResponse<PaginatedResponse<PaymentDto>>> GetPaymentsAsync(PaymentQueryDto query);
        Task<ApiResponse<PaymentDto>> RecordPaymentAsync(RecordPaymentDto request, string userId);
        Task<ApiResponse<List<PaymentDto>>> GetPaymentsByOrderAsync(long orderId);
        Task<ApiResponse<bool>> ReconcilePaymentAsync(ReconcilePaymentDto request, string userId, long? distributorId = null);
        Task<ApiResponse<CashCollectionSummaryDto>> GetCashCollectionSummaryAsync(long? tripId = null, string? dispatcherId = null, DateTime? date = null);
        Task<ApiResponse<List<PaymentReconciliationDto>>> GetUnreconciledPaymentsAsync(long? distributorId = null);
        Task<ApiResponse<decimal>> GetOrderBalanceAsync(long orderId);
    }

    public interface IInvoiceService
    {
        Task<ApiResponse<InvoiceDto>> GetInvoiceByIdAsync(long id);
        Task<ApiResponse<InvoiceDto>> GetInvoiceByOrderIdAsync(long orderId);
        Task<ApiResponse<InvoiceDto>> GenerateInvoiceAsync(GenerateInvoiceDto request, string userId);
        Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(long invoiceId);
        Task<ApiResponse<AccountsReceivableSummaryDto>> GetARSummaryAsync();
        Task<ApiResponse<List<ARAgingLineDto>>> GetARAgingReportAsync();
        Task<ApiResponse<List<InvoiceDto>>> GetOverdueInvoicesAsync();
        Task<ApiResponse<List<InvoiceDto>>> GetInvoicesByStoreAsync(long storeId);
    }
}
