using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Domain.Entities;
using WebApiEvent.Domain.Enums;
using WebApiEvent.Infrastructure.Data;
using WebApiEvent.Infrastructure.Persistence;

namespace WebApiEvent.Presentation.Extensions
{
    public static class DatabaseInitializationExtension
    {
        /// <summary>Применяет миграции, заполняет события и сидирует первого админа.</summary>
        public static void InitializeDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();

            if (!db.Events.Any())
            {
                db.Events.AddRange(SeedData.GetEvents());
                db.SaveChanges();
            }

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
