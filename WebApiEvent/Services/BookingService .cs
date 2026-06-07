using WebApiEvent.CustomExceptions;
using WebApiEvent.Models.DTOs.BookingDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class BookingService : IBookingService
    {
        private readonly List<Booking> _bookings;
        private readonly IEventService _eventService;

        public BookingService(List<Booking> bookings, IEventService eventService)
        {
            _bookings = bookings;
            _eventService = eventService;
        }

        public Task<BookingResponse> CreateBookingAsync(Guid eventId)
        {
            _eventService.GetById(eventId);

            var booking = Booking.CreatePending(eventId);

            lock (_bookings)
            {
                _bookings.Add(booking);
            }

            return Task.FromResult(MapToResponse(booking));
        }

        public Task<BookingResponse> GetBookingAsync(Guid bookingId)
        {
            Booking? booking;
            lock (_bookings)
            {
                booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
            }

            if (booking == null)
                throw new NotFoundException($"Бронь с Id {bookingId} не найдена");

            return Task.FromResult(MapToResponse(booking));
        }

        private static BookingResponse MapToResponse(Booking booking) => new(
            booking.Id,
            booking.EventId,
            booking.Status,
            booking.CreatedAt,
            booking.ProcessedAt
        );
    }
}
