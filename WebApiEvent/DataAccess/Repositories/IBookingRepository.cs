using WebApiEvent.Models.Entity;

namespace WebApiEvent.DataAccess.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid bookingId);
        Task AddAsync(Booking booking);
        Task SaveChangesAsync();
        Task<List<Guid>> GetPendingBookingIdsAsync();
    }
}