using Microsoft.EntityFrameworkCore;
using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.DataAccess.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request)
        {
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize < 1 ? 1 : (request.PageSize > 50 ? 50 : request.PageSize);

            var query = _context.Events.Where(e => e.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
                query = query.Where(e => e.Title.Contains(request.Title));
            if (request.From.HasValue)
                query = query.Where(e => e.StartAt >= request.From.Value);
            if (request.To.HasValue)
                query = query.Where(e => e.EndAt <= request.To.Value);

            int totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventDtoResponse(e.Id, e.Title, e.Description, e.StartAt, e.EndAt))
                .ToListAsync();

            return new PaginatedResult<EventDtoResponse>(items, totalCount, page, pageSize);
        }

        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Event?> GetActiveByIdAsync(Guid id)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task AddAsync(Event eventEntity)
        {
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Event eventEntity)
        {
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}