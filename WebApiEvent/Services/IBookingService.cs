using WebApiEvent.Models.DTOs.BookingDtos;

namespace WebApiEvent.Services
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(Guid eventId);
        Task<BookingResponse> GetBookingAsync(Guid bookingId);
    }
}
