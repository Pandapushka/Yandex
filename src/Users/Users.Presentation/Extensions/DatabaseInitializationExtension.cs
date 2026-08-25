using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Infrastructure.Persistence;

namespace Users.Presentation.Extensions
{
    public static class DatabaseInitializationExtension
    {
        public static void InitializeDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();

            SeedAdmin(scope, app.Configuration);
        }

        private static void SeedAdmin(IServiceScope scope, IConfiguration configuration)
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var login = configuration["SeedAdmin:Login"];
            var password = configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return;

            if (db.Users.Any(u => u.Login == login))
                return;

            var admin = User.Create(login, hasher.Hash(password), UserRole.Admin);
            db.Users.Add(admin);
            db.SaveChanges();
        }
    }
}
