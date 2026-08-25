using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Events.Infrastructure.Data;
using Events.Infrastructure.Persistence;

namespace Events.Presentation.Extensions
{
    public static class DatabaseInitializationExtension
    {
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
        }
    }
}
