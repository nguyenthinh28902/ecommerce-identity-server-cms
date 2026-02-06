using EcommerceIdentityServerCMS.Models.DTOs.SignIn;

namespace EcommerceIdentityServerCMS.Services.Interfaces
{
    public interface IInternalTokenService
    {
        /// <summary>
        /// Lấy token hệ thống (Client Credentials) cho một service cụ thể.
        /// Thường dùng cho các tác vụ background hoặc server-to-server không có user.
        /// </summary>
        Task<TokenResponseDto?> GetSystemTokenAsync();
    }
}
