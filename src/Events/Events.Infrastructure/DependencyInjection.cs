using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Events.Application.Interfaces;
using Events.Infrastructure.Messaging;
using Events.Infrastructure.Persistence;
using Events.Infrastructure.Repositories;

namespace Events.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, string? connectionString, IConfiguration configuration)
        {
            var kafka = configuration.GetSection("Kafka");

            services.AddSingleton(new KafkaOptions
            {
                BootstrapServers = kafka["BootstrapServers"] ?? "localhost:9092",
                ConsumerGroup = kafka["ConsumerGroup"] ?? "events-booking-consumer"
            });

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IEventRepository, EventRepository>();

            services.AddHostedService<KafkaTopicInitializer>();
            services.AddHostedService<BookingConfirmedConsumer>();

            return services;
        }
    }
}
