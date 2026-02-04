using EcommerceIdentityServerCMS.Models.Settings;

namespace EcommerceIdentityServerCMS.Common.Helpers
{
    public static class ConfigAppSetting
    {
        public static IServiceCollection AddConfigAppSetting(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(
             configuration.GetSection("JwtSettings"));
            services.Configure<ServiceAuthOptions>(configuration.GetSection("ServiceAuth"));
            return services;
        }
    }
}
