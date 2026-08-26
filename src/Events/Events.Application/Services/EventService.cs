using Events.Application.DTOs;
using Events.Application.DTOs.Event;
using Events.Application.Interfaces;
using Events.Application.Options;
using Events.Domain.Entities;
using Events.Domain.Exceptions;

namespace Events.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICacheService _cacheService;
        private readonly CacheOptions _cacheOptions;

        public EventService(IEventRepository eventRepository, ICacheService cacheService, CacheOptions cacheOptions)
        {
            _eventRepository = eventRepository;
            _cacheService = cacheService;
            _cacheOptions = cacheOptions;
        }

        public async Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request)
        {
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize < 1 ? 1 : (request.PageSize > 50 ? 50 : request.PageSize);

            var (entities, totalCount) = await _eventRepository.GetActivePagedAsync(
                request.Title, request.From, request.To, page, pageSize);

            var items = entities.Select(ToDto).ToList();

            return new PaginatedResult<EventDtoResponse>(items, totalCount, page, pageSize);
        }

        public async Task<EventDtoResponse> GetByIdAsync(Guid id)
        {
            var cacheKey = CacheKeys.Event(id);

            var cached = await _cacheService.GetAsync<EventDtoResponse>(cacheKey);
            if (cached != null)
                return cached;

            var eventEntity = await _eventRepository.GetActiveByIdAsync(id);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            var dto = ToDto(eventEntity);

            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(_cacheOptions.EventTtlMinutes));

            return dto;
        }

        public async Task<List<EventDtoResponse>> GetTopEventsAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.Top10;

            var cached = await _cacheService.GetAsync<List<EventDtoResponse>>(cacheKey, cancellationToken);
            if (cached != null)
                return cached;

            var events = await _eventRepository.GetTopBySoldPercentageAsync(count, cancellationToken);
            var items = events.Select(ToDto).ToList();

            await _cacheService.SetAsync(cacheKey, items, TimeSpan.FromMinutes(_cacheOptions.TopEventsTtlMinutes), cancellationToken);

            return items;
        }

        public async Task<Guid> CreateAsync(EventDtoRequest request)
        {
            ValidateDates(request.StartAt, request.EndAt);

            var eventEntity = Event.Create(
                request.Title,
                request.Description ?? string.Empty,
                request.StartAt,
                request.EndAt,
                request.AvailableSeats
            );

            _eventRepository.Add(eventEntity);
            await _eventRepository.SaveChangesAsync();
            return eventEntity.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateEventDtoRequest request)
        {
            var existing = await _eventRepository.GetActiveByIdAsync(id);
            if (existing == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            var newTitle = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : existing.Title;
            var newDescription = request.Description ?? existing.Description;
            var newStartAt = request.StartAt ?? existing.StartAt;
            var newEndAt = request.EndAt ?? existing.EndAt;

            ValidateDates(newStartAt, newEndAt);

            existing.Update(newTitle, newDescription, newStartAt, newEndAt);
            await _eventRepository.SaveChangesAsync();

            await _cacheService.RemoveAsync(CacheKeys.Event(id));
        }

        public async Task DeleteAsync(Guid id)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            _eventRepository.Remove(eventEntity);
            await _eventRepository.SaveChangesAsync();

            await _cacheService.RemoveAsync(CacheKeys.Event(id));
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var eventEntity = await _eventRepository.GetActiveByIdAsync(id);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            eventEntity.Deactivate();
            await _eventRepository.SaveChangesAsync();

            await _cacheService.RemoveAsync(CacheKeys.Event(id));
        }

        public async Task DecreaseAvailableSeatsAsync(Guid eventId, int seats, CancellationToken cancellationToken = default)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {eventId} не найдено");

            eventEntity.DecreaseSeats(seats);
            await _eventRepository.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync(CacheKeys.Event(eventId), cancellationToken);
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
            eventEntity.AvailableSeats,
            eventEntity.TotalSeats
        );
    }
}
