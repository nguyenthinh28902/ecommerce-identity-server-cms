using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using EcommerceIdentityServerCMS.Models.Settings;

namespace EcommerceIdentityServerCMS.Services.Interfaces
{
    public interface IInternalTokenService
    {
        /// <summary>
        /// Lấy token hệ thống (Client Credentials) cho một service cụ thể.
        /// Thường dùng cho các tác vụ background hoặc server-to-server không có user.
        /// </summary>
        Task<TokenResponseDto?> GetSystemTokenAsync();

        /// <summary>
        /// Đổi Authorization Code lấy bộ Access Token và Refresh Token từ IdentityServer.
        /// </summary>
        Task<TokenResponseDto?> ExchangeAuthorizationCodeAsync(
            ServiceAuthOptions serviceAuthOptions,
            ExchangeRequest exchangeRequest
            );
    }
}
