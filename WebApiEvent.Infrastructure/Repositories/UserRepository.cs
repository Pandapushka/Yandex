using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Entities;
using WebApiEvent.Infrastructure.Persistence;

namespace WebApiEvent.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public void Add(User user) => context.Add(user);
        

        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => context.SaveChangesAsync(cancellationToken);
    }
}
