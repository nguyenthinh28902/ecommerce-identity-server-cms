using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using EcommerceIdentityServerCMS.Models.ViewModels.Accounts;

namespace EcommerceIdentityServerCMS.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Xác thực thông tin đăng nhập với IdentityCMSService thông qua System Token.
        /// </summary>
        /// <param name="signInViewModel">Thông tin UserID và Password từ nhân sự.</param>
        /// <returns>Thông tin User đã được xác thực dưới dạng SignInResponseDto.</returns>
        Task<Result<SignInResponseDto>> AuthenticateWithUserServiceAsync(SignInViewModel signInViewModel);

        /// <summary>
        /// Thực hiện lưu trữ phiên đăng nhập vào IdentityServer Cookie.
        /// Thiết lập các Claims quan trọng như sub, email, wid và roles.
        /// </summary>
        /// <param name="user">Dữ liệu người dùng từ hệ thống nội bộ.</param>
        Task EstablishUserSessionAsync(SignInResponseDto user);

        /// <summary>
        /// Hàm xác thực thông tin user với IdentityCMSService và thực hiện lưu trữ phiên đăng nhập 
        /// kết kết hợp 2 hàm AuthenticateInternal + SignInIdentityUserAsync
        /// </summary>
        /// <param name="signInViewModel">Thông tin UserID và Password từ nhân sự</param>
        /// <returns></returns>
        public Task<Result<SignInResponseDto>> ProcessSignInAsync(SignInViewModel signInViewModel);

    }
}
