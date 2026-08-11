using Microsoft.EntityFrameworkCore;
using WebApiEvent.CustomExceptions;
using WebApiEvent.DataAccess;
using WebApiEvent.Models.DTOs.BookingDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class BookingService : IBookingService
    {
        private readonly List<Booking> _bookings;
        private readonly List<Event> _events;
        private readonly object _bookingLock = new();

        public BookingService(List<Booking> bookings, IEventService eventService)
        {
            _bookings = bookings;
            _eventService = eventService;
        }

        public async Task<BookingResponse> CreateBookingAsync(Guid eventId)
        {
            _eventService.GetById(eventId);

            var booking = Booking.CreatePending(eventId);

            lock (_bookings)
            {
                _bookings.Add(booking);
            }

            return Task.FromResult(MapToResponse(booking));
        }

        public async Task<BookingResponse> GetBookingAsync(Guid bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new NotFoundException($"Бронь с Id {bookingId} не найдена");

            return MapToResponse(booking);
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