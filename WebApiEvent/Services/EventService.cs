using System.ComponentModel.DataAnnotations;
using WebApiEvent.CustomExceptions;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.Services
{
    public class EventService : IEventService
    {
        private static List<Event> _events = new()
        {
            Event.Create(
                "Конференция разработчиков",
                "Ежегодная конференция по ASP.NET Core",
                new DateTime(2026, 6, 1, 9, 0, 0),
                new DateTime(2026, 6, 1, 18, 0, 0)
            ),
            Event.Create(
                "Митап по C#",
                "Встреча разработчиков для обсуждения лучших практик",
                new DateTime(2026, 6, 15, 18, 0, 0),
                new DateTime(2026, 6, 15, 21, 0, 0)
            )
        };

        public List<EventDtoResponse> GetAll()
        {
            return _events
                .Where(e => e.IsActive)
                .Select(ToDto)
                .ToList();
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