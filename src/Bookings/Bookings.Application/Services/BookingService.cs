using Bookings.Application.DTOs.Booking;
using Bookings.Application.Interfaces;
using Bookings.Domain.Entities;
using Bookings.Domain.Exceptions;

namespace Bookings.Application.Services
{
    public class BookingService : IBookingService
    {
        private const int MaxActiveBookingsPerUser = 10;

        private readonly IBookingRepository _bookingRepository;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingResponse> CreateBookingAsync(Guid userId, Guid eventId, CancellationToken cancellation = default)
        {
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
