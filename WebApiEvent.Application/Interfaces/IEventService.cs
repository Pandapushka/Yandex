using WebApiEvent.Application.DTOs;
using WebApiEvent.Application.DTOs.Event;

namespace WebApiEvent.Application.Interfaces
{
    public interface IEventService
    {
        Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request);
        Task<EventDtoResponse> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(EventDtoRequest request);
        Task UpdateAsync(Guid id, UpdateEventDtoRequest request);
        Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
    }
}
