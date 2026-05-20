using WebApiEvent.Data;
using WebApiEvent.Extentions;
using WebApiEvent.Models.Entity;
using WebApiEvent.Services;

namespace WebApiEvent
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddCorsPolicyCustom();
            services.AddSingleton<IEventService>(sp => new EventService(SeedData.GetEvents()));
            services.AddSingleton<List<Booking>>(sp => new List<Booking>());
            services.AddScoped<IBookingService, BookingService>();
            services.AddHostedService<BookingProcessingService>();
            return services;
        }
    }
}
