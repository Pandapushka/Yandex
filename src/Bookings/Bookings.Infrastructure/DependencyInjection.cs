using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Bookings.Application.Interfaces;
using Bookings.Infrastructure.Messaging;
using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;

namespace Bookings.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, string? connectionString, IConfiguration configuration)
        {
            var kafka = configuration.GetSection("Kafka");

            services.AddSingleton(new KafkaOptions
            {
                BootstrapServers = kafka["BootstrapServers"] ?? "localhost:9092"
            });

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

            return services;
        }
    }
}
