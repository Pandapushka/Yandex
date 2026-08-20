using WebApiEvent.Domain.Entities;

namespace WebApiEvent.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<List<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken = default);

        void Add(Booking booking);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
