using BookingContracts;

namespace Bookings.Application.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync(BookingConfirmed bookingConfirmed, CancellationToken cancellationToken = default);
    }
}
