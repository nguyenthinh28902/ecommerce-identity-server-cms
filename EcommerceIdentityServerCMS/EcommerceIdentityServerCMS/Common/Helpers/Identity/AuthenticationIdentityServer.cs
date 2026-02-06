using EcommerceIdentityServerCMS.Models.Settings;
using EcommerceIdentityServerCMS.Services.Interfaces;
using EcommerceIdentityServerCMS.Services.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public static class AuthenticationIdentityServer
{
    public static IServiceCollection AddAuthenticationIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var migrationsAssembly = typeof(AuthenticationIdentityServer).GetTypeInfo().Assembly.GetName().Name;
        var connectionString = configuration.GetConnectionString("EcomCMS_IdentityServer_DB");
        var configServiceUrl = configuration.GetSection(nameof(ConfigServiceUrl)).Get<ConfigServiceUrl>();
        if (configServiceUrl == null || connectionString == null)
            throw new ArgumentNullException($"Không tìm thấy cấu hình trong appsettings.{nameof(AddAuthenticationIdentityServer)}");
        var EcommerceMVCCMS = configServiceUrl.EcommerceMVCCMS;
        var RedisConnectionString = configuration["RedisConnection:RedisConnectionString"];
        var InstanceName = configuration["RedisConnection:InstanceName"];

        // 1. Cấu hình Redis Cache cho IdentityServer (Lưu trữ các Grant, Token, v.v.)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = RedisConnectionString;
            options.InstanceName = InstanceName;
        });

        var builder = services.AddIdentityServer(options =>
        {

            // --- CẤU HÌNH ĐƯỜNG DẪN LOGIN TẠI ĐÂY ---
            options.UserInteraction.LoginUrl = "/auth/dang-nhap-he-thong"; // Đường dẫn đến Controller/Page Login của bạn
            options.UserInteraction.LogoutUrl = "/auth/dang-xuat-he-thong";
            options.UserInteraction.ErrorUrl = "/auth/dang-nhap-that-bai";

            options.KeyManagement.Enabled = false; // Vẫn giữ false vì bạn dùng DeveloperSigningCredential
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
        })
        // 2. Cấu hình Database cho Configuration Store (Clients, Resources, Scopes)
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        // 3. Cấu hình Database cho Operational Store (Tokens, Codes, Consents)
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));

            // Tự động dọn dẹp các token đã hết hạn trong DB
            options.EnableTokenCleanup = true;
            options.TokenCleanupInterval = 3600; // 1 giờ dọn 1 lần
        })
        .AddDeveloperSigningCredential()
        .AddProfileService<GatewayUserProfileService>();

        // Đăng ký Service
        services.AddScoped<IClientService, ClientService>();

        return services;
    }
}