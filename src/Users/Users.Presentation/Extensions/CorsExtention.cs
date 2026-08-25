using Microsoft.Extensions.DependencyInjection;

namespace Users.Presentation.Extensions
{
    public static class CorsExtention
    {
        public static IServiceCollection AddCorsPolicyCustom(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Prod", policy =>
                {
                    policy.WithOrigins("")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            return services;
        }
    }
}
