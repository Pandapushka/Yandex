using Microsoft.EntityFrameworkCore;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Entities;
using WebApiEvent.Domain.Enums;
using WebApiEvent.Infrastructure.Persistence;

namespace WebApiEvent.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<List<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);
        }

        public void Add(Booking booking) => _context.Bookings.Add(booking);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);

        public async Task<int> CountActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var count = await _context.Bookings.CountAsync(b=> b.UserId == userId && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed), cancellationToken);
            return count;
        }
    }
}
