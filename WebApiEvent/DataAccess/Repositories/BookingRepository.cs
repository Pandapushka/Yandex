using Microsoft.EntityFrameworkCore;
using WebApiEvent.Models.Entity;
using WebApiEvent.Models.Enums;

namespace WebApiEvent.DataAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid bookingId)
        {
            return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task AddAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetPendingBookingIdsAsync()
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .Select(b => b.Id)
                .ToListAsync();
        }
    }
}