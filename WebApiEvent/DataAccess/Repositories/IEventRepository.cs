using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;

namespace WebApiEvent.DataAccess.Repositories
{
    public interface IEventRepository
    {
        Task<PaginatedResult<EventDtoResponse>> GetAllAsync(EventRequestDto request);
        Task<Event?> GetByIdAsync(Guid id);
        Task<Event?> GetActiveByIdAsync(Guid id);
        Task AddAsync(Event eventEntity);
        Task DeleteAsync(Event eventEntity);
        Task SaveChangesAsync();
    }
}