using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using EcommerceIdentityServerCMS.Common.Helpers.Identity.Config;
using EcommerceIdentityServerCMS.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace EcommerceIdentityServerCMS.Common.Helpers.Identity
{
    public static class DatabaseInitializer
    {
        public static async Task InitDatabaseAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();

            // Lấy các Context cần thiết
            var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            var persistedContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            var configServiceUrl = configuration.GetSection(nameof(ConfigServiceUrl)).Get<ConfigServiceUrl>();
            if (configServiceUrl == null)
                throw new ArgumentNullException($"Không tìm thấy cấu hình trong appsettings.{nameof(InitDatabaseAsync)}");
            // 1. Tự động chạy Migration nếu DB chưa có bảng
            await configContext.Database.MigrateAsync();
            await persistedContext.Database.MigrateAsync();

            // 2. Seed Identity Resources (OpenId, Profile, Email)
            if (!await configContext.IdentityResources.AnyAsync())
            {
                foreach (var resource in IdentityResourcesConfig.Get())
                {
                    configContext.IdentityResources.Add(resource.ToEntity());
                }
                await configContext.SaveChangesAsync();
            }

            // 3. Seed Api Scopes (Kiểm tra từng cái để thêm mới nếu thiếu)
            var existingScopes = await configContext.ApiScopes.Select(s => s.Name).ToListAsync();
            foreach (var scopeItem in ApiScopes.Get())
            {
                if (!existingScopes.Contains(scopeItem.Name))
                {
                    configContext.ApiScopes.Add(scopeItem.ToEntity());
                }
            }
            await configContext.SaveChangesAsync();

            // 4. Seed Api Resources
            var existingResources = await configContext.ApiResources.Select(r => r.Name).ToListAsync();
            foreach (var resource in ApiResources.Get())
            {
                if (!existingResources.Contains(resource.Name))
                {
                    configContext.ApiResources.Add(resource.ToEntity());
                }
            }
            await configContext.SaveChangesAsync();

            // 5. Seed Clients

            var existingClients = await configContext.Clients.Select(c => c.ClientId).ToListAsync();
            foreach (var client in Clients.Get(configServiceUrl))
            {
                if (!existingClients.Contains(client.ClientId))
                {
                    configContext.Clients.Add(client.ToEntity());
                }
            }
            await configContext.SaveChangesAsync();
        }
    }
}