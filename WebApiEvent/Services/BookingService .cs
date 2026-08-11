using WebApiEvent.CustomExceptions;
using WebApiEvent.DataAccess.Repositories;
using WebApiEvent.Models.DTOs.BookingDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventService _eventService;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public BookingService(IBookingRepository bookingRepository, IEventService eventService)
        {
            _bookingRepository = bookingRepository;
            _eventService = eventService;
        }

        public async Task<BookingResponse> CreateBookingAsync(Guid eventId)
        {
            await _eventService.GetByIdAsync(eventId);

            var booking = Booking.CreatePending(eventId);

            await _semaphore.WaitAsync();
            try
            {
                await _bookingRepository.AddAsync(booking);
            }
            finally
            {
                _semaphore.Release();
            }

            return MapToResponse(booking);
        }

        public async Task<BookingResponse> GetBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
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