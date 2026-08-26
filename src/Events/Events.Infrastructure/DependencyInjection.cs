using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Events.Application.Interfaces;
using Events.Application.Options;
using Events.Infrastructure.Caching;
using Events.Infrastructure.Messaging;
using Events.Infrastructure.Persistence;
using Events.Infrastructure.Repositories;
using StackExchange.Redis;

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

            AddRedis(services, configuration);

            services.AddHostedService<KafkaTopicInitializer>();
            services.AddHostedService<BookingConfirmedConsumer>();

            return services;
        }

        private static void AddRedis(IServiceCollection services, IConfiguration configuration)
        {
            var redis = configuration.GetSection("Redis");

            var cacheOptions = new CacheOptions
            {
                EventTtlMinutes = ReadInt(redis["EventTtlMinutes"], 5),
                TopEventsTtlMinutes = ReadInt(redis["TopEventsTtlMinutes"], 10)
            };
            services.AddSingleton(cacheOptions);

            var connectionString = redis["ConnectionString"] ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });

            services.AddSingleton<ICacheService, RedisCacheService>();
        }

        private static int ReadInt(string? value, int defaultValue)
            => int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }
}
