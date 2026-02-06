using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.Enums;
using EcommerceIdentityServerCMS.Models.Settings;

namespace EcommerceIdentityServerCMS.Common.Helpers
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            var configServiceUrl = configuration.GetSection(nameof(ConfigServiceUrl)).Get<ConfigServiceUrl>();
            if (configServiceUrl == null) throw new ArgumentNullException($"Không tìm thấy cấu hình trong appsettings.{nameof(AddAuthenticationExtensions)}");
            services.AddCors(options =>
            {
                options.AddPolicy(AuthEnum.AllowWebCMS.ToString(), policy =>
                {
                    var EcommerceMVCCMS = configServiceUrl.EcommerceMVCCMS;
                    if (!string.IsNullOrEmpty(EcommerceMVCCMS))
                    {
                        policy.WithOrigins(EcommerceMVCCMS)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                });
            });
            var hours = (int)ExpireTimeSpanSignIn.Medium; // hours = 8
            // 3. Cấu hình Cookie (Để IdentityServer tương tác tốt với trình duyệt hiện đại)
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "identity_auth_session";
                // Đổi về Lax vì đi qua Gateway/Cùng Domain chính sẽ an toàn và dễ chịu hơn
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(hours);
                options.SlidingExpiration = true;
            });

            return services;
        }
    }
}
