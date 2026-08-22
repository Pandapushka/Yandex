using System.Threading;
using WebApiEvent.Application.DTOs.Booking;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Entities;
using WebApiEvent.Domain.Exceptions;

namespace WebApiEvent.Application.Services
{
    public class BookingService : IBookingService
    {
        private const int MaxActiveBookingsPerUser = 10;

        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
        }

        public async Task<BookingResponse> CreateBookingAsync(Guid userId, Guid eventId, CancellationToken cancellation = default)
        {
            var eventEntity = await _eventRepository.GetActiveByIdAsync(eventId, cancellation);
            
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {eventId} не найдено");

            if(eventEntity.StartAt <= DateTime.UtcNow)
                throw new EventAlreadyStartedException("Нельзя забронировать событие, которое уже началось");

            await _semaphore.WaitAsync(cancellation);
            try
            {
                var activeCount = await _bookingRepository.CountActiveByUserAsync(userId, cancellation);
                if (activeCount >= MaxActiveBookingsPerUser)
                    throw new BookingLimitExceededException(MaxActiveBookingsPerUser);

                var booking = Booking.CreatePending(userId, eventId);
                _bookingRepository.Add(booking);
                await _bookingRepository.SaveChangesAsync(cancellation);

                return MapToResponse(booking);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<BookingResponse> GetBookingAsync(
            Guid bookingId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var booking = await GetBookingByIdAsync(bookingId, cancellationToken);

            if (!isAdmin && booking.UserId != currentUserId)
                throw new ForbiddenException("Недостаточно прав для просмотра чужой брони");

            return MapToResponse(booking);
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
        {
            var booking = await GetBookingByIdAsync(bookingId, cancellationToken);

            if (!isAdmin && booking.UserId != currentUserId)
                throw new ForbiddenException("Недостаточно прав для отмены чужой брони");

            var eventEntity = await _eventRepository.GetByIdAsync(booking.EventId, cancellationToken);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {booking.EventId} не найдено");

            if (eventEntity.StartAt <= DateTime.UtcNow)
                throw new EventAlreadyStartedException("Нельзя отменить бронь после начала события");

            booking.Cancel();
            await _bookingRepository.SaveChangesAsync(cancellationToken);
        }
        private async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (booking == null)
                throw new NotFoundException($"Бронь с Id {bookingId} не найдена");
            return booking;
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
