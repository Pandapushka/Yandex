using Events.Application.DTOs;
using Events.Application.DTOs.Event;

namespace Events.Application.Interfaces
{
    public interface IEventService
    {
        Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request);
        Task<EventDtoResponse> GetByIdAsync(Guid id);
        Task<List<EventDtoResponse>> GetTopEventsAsync(int count = 10, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(EventDtoRequest request);
        Task UpdateAsync(Guid id, UpdateEventDtoRequest request);
        Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        Task DecreaseAvailableSeatsAsync(Guid eventId, int seats, CancellationToken cancellationToken = default);
    }
}
