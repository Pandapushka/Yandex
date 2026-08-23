using WebApiEvent.Application.DTOs.Booking;

namespace WebApiEvent.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
        Task<BookingResponse> GetBookingAsync(
            Guid bookingId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
        Task CancelBookingAsync(Guid bookingId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    }
}
