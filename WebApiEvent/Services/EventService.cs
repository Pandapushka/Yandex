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
            try
            {
                return _events
                    .Where(e => e.IsActive)
                    .Select(ToDto)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Ошибка при получении списка событий: {ex.Message}");
            }
        }

        public EventDtoResponse? GetById(Guid id)
        {
            try
            {
                var eventEntity = _events.FirstOrDefault(e => e.Id == id && e.IsActive);
                return eventEntity != null ? ToDto(eventEntity) : null;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Ошибка при получении события с Id {id} {ex.Message}");
            }
        }

        public Guid Create(EventDtoRequest request)
        {
            try
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
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Ошибка при создании события {ex.Message}");
            }
        }

        public void Update(Guid id, UpdateEventDtoRequest request)
        {
            try
            {
                var existing = _events.FirstOrDefault(e => e.Id == id && e.IsActive);
                if (existing == null)
                    throw new ServiceException($"Событие с Id {id} не найдено");

                var newTitle = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : existing.Title;
                var newDescription = request.Description ?? existing.Description;
                var newStartAt = request.StartAt ?? existing.StartAt;
                var newEndAt = request.EndAt ?? existing.EndAt;

                ValidateDates(newStartAt, newEndAt);

                existing.Update(newTitle, newDescription, newStartAt, newEndAt);
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Ошибка при обновлении события с Id {id}: {ex.Message}");
            }
        }

        public void Delete(Guid id)
        {
            try
            {
                var eventEntity = _events.FirstOrDefault(e => e.Id == id);
                if (eventEntity == null)
                    throw new ServiceException($"Событие с Id {id} не найдено");

                _events.Remove(eventEntity);
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Ошибка при удалении события с Id {id}: {ex.Message}");
            }
        }

        public void SoftDelete(Guid id)
        {
            try
            {
                var eventEntity = _events.FirstOrDefault(e => e.Id == id && e.IsActive);
                if (eventEntity == null)
                    throw new ServiceException($"Событие с Id {id} не найдено");

                eventEntity.Deactivate();
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Ошибка при мягком удалении события с Id {id}: {ex.Message}");
            }
        }

        private static void ValidateDates(DateTime startAt, DateTime endAt)
        {
            if (startAt >= endAt)
                throw new ServiceException("Дата окончания должна быть позже даты начала");
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
