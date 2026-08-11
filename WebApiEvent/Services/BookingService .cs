using Microsoft.EntityFrameworkCore;
using WebApiEvent.CustomExceptions;
using WebApiEvent.DataAccess;
using WebApiEvent.Models.DTOs.BookingDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private readonly IEventService _eventService;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public BookingService(AppDbContext context, IEventService eventService)
        {
            _context = context;
            _eventService = eventService;
        }

        public async Task<BookingResponse> CreateBookingAsync(Guid eventId)
        {
            await _eventService.GetByIdAsync(eventId);

            var booking = Booking.CreatePending(eventId);

            await _semaphore.WaitAsync();
            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }

            return MapToResponse(booking);
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