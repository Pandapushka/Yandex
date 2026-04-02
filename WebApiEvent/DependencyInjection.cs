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
            services.AddScoped<IEventService, EventService>();
            return services;
        }
    }
}
