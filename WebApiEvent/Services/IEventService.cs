using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;

namespace WebApiEvent.Services
{
    public interface IEventService
    {
        PaginatedResult<EventDtoResponse> GetAll(EventRequestDto request);
        EventDtoResponse? GetById(Guid id);
        Guid Create(EventDtoRequest request);
        void Update(Guid id, UpdateEventDtoRequest request);
        void Delete(Guid id);
        void SoftDelete(Guid id);
    }
}
