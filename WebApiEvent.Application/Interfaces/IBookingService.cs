using WebApiEvent.Application.DTOs.Booking;

namespace WebApiEvent.Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(Guid eventId);
        Task<BookingResponse> GetBookingAsync(Guid bookingId);
    }
}
