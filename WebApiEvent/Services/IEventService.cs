using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;

namespace WebApiEvent.Services
{
    public interface IEventService
    {
        Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request);
        Task<EventDtoResponse?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(EventDtoRequest request);
        Task UpdateAsync(Guid id, UpdateEventDtoRequest request);
        Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
    }
}