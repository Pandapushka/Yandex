using Microsoft.EntityFrameworkCore;
using WebApiEvent.DataAccess;
using WebApiEvent.Extentions;
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

            services.AddSingleton<List<Event>>(sp => SeedData.GetEvents());
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<List<Booking>>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddHostedService<BookingProcessingService>();

            return services;
        }
    }
}