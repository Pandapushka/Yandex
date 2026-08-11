using WebApiEvent.CustomExceptions;
using WebApiEvent.DataAccess;
using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(List<Event>? events = null)
        {
            _events = events ?? SeedData.GetEvents();
        }

        public async Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request)
        {
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize < 1 ? 1 : (request.PageSize > 50 ? 50 : request.PageSize);
            var title = request.Title;
            var from = request.From;
            var to = request.To;

            var query = _context.Events.Where(e => e.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(e => e.Title.Contains(title));
            if (from.HasValue)
                query = query.Where(e => e.StartAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.EndAt <= to.Value);

            int totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventDtoResponse(e.Id, e.Title, e.Description, e.StartAt, e.EndAt))
                .ToListAsync();

            return new PaginatedResult<EventDtoResponse>(items, totalCount, page, pageSize);
        }

        public async Task<EventDtoResponse?> GetByIdAsync(Guid id)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");
            return ToDto(eventEntity);
        }

        public async Task<Guid> CreateAsync(EventDtoRequest request)
        {
            ValidateDates(request.StartAt, request.EndAt);

            var eventEntity = Event.Create(
                request.Title,
                request.Description ?? string.Empty,
                request.StartAt,
                request.EndAt,
                request.TotalSeats
            );

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();
            return eventEntity.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateEventDtoRequest request)
        {
            var existing = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (existing == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            var newTitle = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : existing.Title;
            var newDescription = request.Description ?? existing.Description;
            var newStartAt = request.StartAt ?? existing.StartAt;
            var newEndAt = request.EndAt ?? existing.EndAt;

            ValidateDates(newStartAt, newEndAt);

            existing.Update(newTitle, newDescription, newStartAt, newEndAt);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            eventEntity.Deactivate();
            await _context.SaveChangesAsync();
        }

        private static void ValidateDates(DateTime startAt, DateTime endAt)
        {
            if (startAt >= endAt)
                throw new CustomValidationException("Дата окончания должна быть позже даты начала");
        }

        private static EventDtoResponse ToDto(Event eventEntity) => new(
            eventEntity.Id,
            eventEntity.Title,
            eventEntity.Description,
            eventEntity.StartAt,
            eventEntity.EndAt,
            eventEntity.TotalSeats,
            eventEntity.AvailableSeats
        );
    }
}