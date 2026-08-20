using WebApiEvent.Application.DTOs;
using WebApiEvent.Application.DTOs.Event;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Entities;
using WebApiEvent.Domain.Exceptions;

namespace WebApiEvent.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request)
        {
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize < 1 ? 1 : (request.PageSize > 50 ? 50 : request.PageSize);

            var (entities, totalCount) = await _eventRepository.GetActivePagedAsync(
                request.Title, request.From, request.To, page, pageSize);

            var items = entities
                .Select(e => new EventDtoResponse(e.Id, e.Title, e.Description, e.StartAt, e.EndAt))
                .ToList();

            return new PaginatedResult<EventDtoResponse>(items, totalCount, page, pageSize);
        }

        public async Task<EventDtoResponse> GetByIdAsync(Guid id)
        {
            var eventEntity = await _eventRepository.GetActiveByIdAsync(id);
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
                request.EndAt
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
        }

        public async Task DeleteAsync(Guid id)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            _eventRepository.Remove(eventEntity);
            await _eventRepository.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var eventEntity = await _eventRepository.GetActiveByIdAsync(id);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            eventEntity.Deactivate();
            await _eventRepository.SaveChangesAsync();
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
            eventEntity.EndAt
        );
    }
}
