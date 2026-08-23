using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEvent.Domain.Entities;

namespace WebApiEvent.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);

        void Add(User user);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
