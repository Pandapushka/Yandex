using Microsoft.Extensions.DependencyInjection;
using Events.Application.Interfaces;
using Events.Application.Services;

namespace Events.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            return services;
        }
    }
}
