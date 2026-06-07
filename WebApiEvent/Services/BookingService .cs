using WebApiEvent.CustomExceptions;
using WebApiEvent.Models.DTOs.BookingDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class BookingService : IBookingService
    {
        private readonly List<Booking> _bookings;
        private readonly List<Event> _events;
        private readonly object _bookingLock = new();

        public BookingService(List<Booking> bookings, List<Event> events)
        {
            _bookings = bookings;
            _events = events;
        }

        public Task<BookingResponse> CreateBookingAsync(Guid eventId)
        {
            lock (_bookingLock)
            {
                var eventEntity = _events.FirstOrDefault(e => e.Id == eventId && e.IsActive);
                if (eventEntity == null)
                    throw new NotFoundException($"Событие с Id {eventId} не найдено");

                if (!eventEntity.TryReserveSeats(1))
                    throw new NoAvailableSeatsException("Нет свободных мест для этого события");

                var booking = Booking.CreatePending(eventId);
                _bookings.Add(booking);
                return Task.FromResult(MapToResponse(booking));
            }
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