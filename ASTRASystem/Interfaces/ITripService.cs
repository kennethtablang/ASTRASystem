using ASTRASystem.DTO.Common;
using ASTRASystem.DTO.Delivery;
using ASTRASystem.DTO.Trip;

namespace ASTRASystem.Interfaces
{
    public interface ITripService
    {
        Task<ApiResponse<TripDto>> GetTripByIdAsync(long id);
        Task<ApiResponse<PaginatedResponse<TripListItemDto>>> GetTripsAsync(TripQueryDto query);
        Task<ApiResponse<TripDto>> CreateTripAsync(CreateTripDto request, string userId);
        Task<ApiResponse<TripDto>> UpdateTripAsync(UpdateTripDto request, string userId);
        Task<ApiResponse<TripDto>> UpdateTripStatusAsync(UpdateTripStatusDto request, string userId);
        Task<ApiResponse<bool>> ReorderTripAssignmentsAsync(ReorderTripAssignmentsDto request, string userId);
        Task<ApiResponse<bool>> CancelTripAsync(long tripId, string userId, string? reason);
        Task<ApiResponse<TripManifestDto>> GetTripManifestAsync(long tripId);
        Task<ApiResponse<byte[]>> GenerateTripManifestPdfAsync(long tripId);
        Task<ApiResponse<List<TripDto>>> GetActiveTripsAsync(string? dispatcherId = null);
        Task<ApiResponse<List<long>>> SuggestTripSequenceAsync(List<long> orderIds);
    }

    public interface IDeliveryService
    {
        Task<ApiResponse<DeliveryPhotoDto>> UploadDeliveryPhotoAsync(UploadDeliveryPhotoDto request, string userId);
        Task<ApiResponse<List<DeliveryPhotoDto>>> GetDeliveryPhotosAsync(long orderId);
        Task<ApiResponse<bool>> UpdateLocationAsync(LocationUpdateDto request, string userId);
        Task<ApiResponse<LiveTripTrackingDto>> GetLiveTripTrackingAsync(long tripId);
        Task<ApiResponse<bool>> MarkOrderAsDeliveredAsync(MarkDeliveredDto request, string userId);
        Task<ApiResponse<DeliveryExceptionDto>> ReportDeliveryExceptionAsync(ReportDeliveryExceptionDto request, string userId);
        Task<ApiResponse<bool>> RecordDeliveryAttemptAsync(DeliveryAttemptDto request, string userId);
        Task<ApiResponse<List<DeliveryExceptionDto>>> GetDeliveryExceptionsAsync(long? orderId = null);
    }
}
