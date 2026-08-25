using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);

        void Add(User user);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
