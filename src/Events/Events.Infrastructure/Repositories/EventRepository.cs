using Microsoft.EntityFrameworkCore;
using Events.Application.Interfaces;
using Events.Domain.Entities;
using Events.Infrastructure.Persistence;

namespace Events.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Event> Items, int TotalCount)> GetActivePagedAsync(
            string? title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Events.Where(e => e.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(e => e.Title.Contains(title));
            if (from.HasValue)
                query = query.Where(e => e.StartAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.EndAt <= to.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Event?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.IsActive, cancellationToken);
        }

        public async Task<List<Event>> GetTopBySoldPercentageAsync(int count, CancellationToken cancellationToken = default)
        {
            var events = await _context.Events
                .Where(e => e.IsActive && e.TotalSeats > 0)
                .ToListAsync(cancellationToken);

            return events
                .OrderByDescending(e => (e.TotalSeats - e.AvailableSeats) / (double)e.TotalSeats)
                .ThenByDescending(e => e.StartAt)
                .Take(count)
                .ToList();
        }

        public void Add(Event eventEntity) => _context.Events.Add(eventEntity);

        public void Remove(Event eventEntity) => _context.Events.Remove(eventEntity);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }
}
