using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiEvent.DataAccess;
using WebApiEvent.DataAccess.Repositories;
using WebApiEvent.Extentions;
using WebApiEvent.Services;

namespace WebApiEvent
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddCorsPolicyCustom();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();

            services.AddHostedService<BookingProcessingService>();

            return services;
        }
    }
}