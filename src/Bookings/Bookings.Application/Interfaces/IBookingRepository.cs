using Bookings.Domain.Entities;

namespace Bookings.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<List<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken = default);

        Task<int> CountActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        void Add(Booking booking);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
