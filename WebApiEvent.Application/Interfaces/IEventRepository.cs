using WebApiEvent.Domain.Entities;

namespace WebApiEvent.Application.Interfaces
{
    public interface IEventRepository
    {
        Task<(List<Event> Items, int TotalCount)> GetActivePagedAsync(
            string? title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Event?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Add(Event eventEntity);

        void Remove(Event eventEntity);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
