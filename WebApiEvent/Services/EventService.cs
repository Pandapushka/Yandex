using System.ComponentModel.DataAnnotations;
using WebApiEvent.CustomExceptions;
using WebApiEvent.Data;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class EventService : IEventService
    {
        private static List<Event> _events = SeedData.GetEvents();

        public List<EventDtoResponse> GetAll(string? title = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _events.Where(e => e.IsActive);


            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (from.HasValue)
                query = query.Where(e => e.StartAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.EndAt <= to.Value);

            return query.Select(ToDto).ToList();
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