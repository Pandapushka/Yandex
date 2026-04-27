using WebApiEvent.Models.DTOs.EventDtos;

namespace WebApiEvent.Services
{
    public interface IEventService
    {
        List<EventDtoResponse> GetAll(string? title = null, DateTime? from = null, DateTime? to = null);
        EventDtoResponse? GetById(Guid id);
        Guid Create(EventDtoRequest request);
        void Update(Guid id, UpdateEventDtoRequest request);
        void Delete(Guid id);
        void SoftDelete(Guid id);
    }
}
