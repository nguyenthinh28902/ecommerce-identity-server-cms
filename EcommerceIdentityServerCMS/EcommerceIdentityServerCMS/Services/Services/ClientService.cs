using Duende.IdentityServer.EntityFramework.DbContexts;
using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class ClientService : IClientService
    {
        private readonly ConfigurationDbContext _context; // DbContext trỏ tới DB Identity Server của bạn

        public ClientService(ConfigurationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetAllowedScopesAsync(string clientId)
        {
            // Truy vấn Client và nạp các Scope tương ứng
            var client = await _context.Clients
                .Include(c => c.AllowedScopes)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (client == null || client.AllowedScopes == null)
            {
                return "openid profile";
            }

            // Lấy danh sách Scope từ bảng phụ
            var scopes = client.AllowedScopes.Select(s => s.Scope).ToList();

            return string.Join(" ", scopes);
        }
    }
}
