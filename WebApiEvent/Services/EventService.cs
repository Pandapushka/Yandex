using System.ComponentModel.DataAnnotations;
using WebApiEvent.CustomExceptions;
using WebApiEvent.Data;
using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events;

        public EventService(List<Event>? events = null)
        {
            _events = events ?? SeedData.GetEvents();
        }

        public PaginatedResult<EventDtoResponse> GetAll(EventRequestDto request)
        {
            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize < 1 ? 1 : (request.PageSize > 50 ? 50 : request.PageSize);
            var title = request.Title;
            var from = request.From;
            var to = request.To;

            var query = _events.Where(e => e.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            if (from.HasValue)
                query = query.Where(e => e.StartAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.EndAt <= to.Value);

            int totalCount = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDto)
                .ToList();

            return new PaginatedResult<EventDtoResponse>(items, totalCount, page, pageSize);
        }

        public EventDtoResponse? GetById(Guid id)
        {
            var eventEntity = _events.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");
            return ToDto(eventEntity);
        }

        public Guid Create(EventDtoRequest request)
        {
            ValidateDates(request.StartAt, request.EndAt);

            var eventEntity = Event.Create(
                request.Title,
                request.Description ?? string.Empty,
                request.StartAt,
                request.EndAt
            );

            _events.Add(eventEntity);
            return eventEntity.Id;
        }

        public void Update(Guid id, UpdateEventDtoRequest request)
        {
            var existing = _events.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (existing == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            var newTitle = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : existing.Title;
            var newDescription = request.Description ?? existing.Description;
            var newStartAt = request.StartAt ?? existing.StartAt;
            var newEndAt = request.EndAt ?? existing.EndAt;

            ValidateDates(newStartAt, newEndAt);

            existing.Update(newTitle, newDescription, newStartAt, newEndAt);
        }

        public void Delete(Guid id)
        {
            var eventEntity = _events.FirstOrDefault(e => e.Id == id);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            _events.Remove(eventEntity);
        }

        public void SoftDelete(Guid id)
        {
            var eventEntity = _events.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (eventEntity == null)
                throw new NotFoundException($"Событие с Id {id} не найдено");

            eventEntity.Deactivate();
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