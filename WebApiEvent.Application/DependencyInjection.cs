using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Application.Services;

namespace WebApiEvent.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddHostedService<BookingProcessingService>();

            return services;
        }
    }
}
