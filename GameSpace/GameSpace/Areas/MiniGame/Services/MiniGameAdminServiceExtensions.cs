using Microsoft.Extensions.DependencyInjection;

namespace GameSpace.Areas.MiniGame.Services
{
    public static class MiniGameAdminServiceExtensions
    {
        public static IServiceCollection AddMiniGameAdminServices(this IServiceCollection services)
        {
            services.AddScoped<IMiniGameAdminService, MiniGameAdminService>();
            services.AddScoped<IMiniGameAdminAuthService, MiniGameAdminAuthService>();
            services.AddScoped<IMiniGameAdminGate, MiniGameAdminGate>();
            return services;
        }
    }
}

