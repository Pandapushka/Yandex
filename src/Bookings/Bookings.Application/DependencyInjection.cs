using Microsoft.Extensions.DependencyInjection;
using Bookings.Application.Interfaces;
using Bookings.Application.Services;

namespace Bookings.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IBookingService, BookingService>();
            services.AddHostedService<BookingProcessingService>();
            return services;
        }
    }
}
