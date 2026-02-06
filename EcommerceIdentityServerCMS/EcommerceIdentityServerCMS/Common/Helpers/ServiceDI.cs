using EcommerceIdentityServerCMS.Models.Settings;
using EcommerceIdentityServerCMS.Services.Interfaces;
using EcommerceIdentityServerCMS.Services.Services;

namespace EcommerceIdentityServerCMS.Common.Helpers
{
    public static class ServiceDI
    {
        public static IServiceCollection AddServiceDI(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind cấu hình vào Model
            var configServiceUrl = configuration.GetSection(nameof(ConfigServiceUrl)).Get<ConfigServiceUrl>();
            var serviceAuthOptions = configuration.GetSection(nameof(InternalAuthOptions)).Get<InternalAuthOptions>();
            if (configServiceUrl == null || serviceAuthOptions == null) throw new ArgumentNullException($"Không tìm thấy cấu hình trong appsettings.{nameof(AddServiceDI)}");
            services.Configure<InternalAuthHeader>(configuration.GetSection("InternalAuthHeader"));

            // Lấy giá trị ra để cấu hình HttpClient
            var authSettings = configuration.GetSection("InternalAuthHeader").Get<InternalAuthHeader>();
            services.AddSingleton<IInternalCacheService, InternalCacheService>();
            services.AddHttpClient<IInternalTokenService, InternalTokenService>(client =>
            {
                client.BaseAddress = new Uri(serviceAuthOptions.TokenEndpoint);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler // TẠO MỚI Ở ĐÂY
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            });
            services.AddHttpClient<IAuthService, AuthService>(client =>
            {
                client.BaseAddress = new Uri(configServiceUrl.EcommerceGatewayCMS);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
                // Bỏ qua check SSL trong môi trường dev
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            });


            return services;
        }
    }
}
