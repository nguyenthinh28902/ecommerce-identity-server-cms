using Duende.IdentityServer.Services;
using EcommerceIdentityServerCMS.Common.Exceptions;
using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.ViewModels.Accounts;
using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace EcommerceIdentityServerCMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IInternalCacheService _internalCacheService;
        public AuthController(IAuthService authService, IIdentityServerInteractionService interaction, ILogger<AuthController> logger
            , IInternalCacheService internalCacheService)
        {
            _authService = authService;
            _interaction = interaction;
            _logger = logger;
            _internalCacheService = internalCacheService;
        }

        /// <summary>
        /// form login
        /// </summary>
        /// <param name="signInViewModel"></param>
        /// <returns>call back identiy để thực hiện login</returns>
        [HttpPost("dang-nhap")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([FromForm] SignInViewModel signInViewModel)
        {
            var context = await _interaction.GetAuthorizationContextAsync(signInViewModel.ReturnUrl);
            bool isLocal = Url.IsLocalUrl(signInViewModel.ReturnUrl);

            if (context == null && !isLocal)
            {
                return BadRequest(new { message = "Yêu cầu điều hướng không hợp lệ hoặc không an toàn." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            try
            {
                var result = await _authService.ProcessSignInAsync(signInViewModel);

                if (!result.IsSuccess)
                {
                    // Trả về lỗi nghiệp vụ (Sai pass, user bị khóa...)
                    return Unauthorized(new { message = result.Error ?? "Tài khoản hoặc mật khẩu không chính xác." });
                }

                return Redirect(signInViewModel.ReturnUrl ?? "/");
            }
            catch (UnauthorizedException ex)
            {
                // Bắt lỗi hệ thống nội bộ hỏng (Token CMS lỗi...) mà không làm crash app
                _logger.LogCritical(ex, "Lỗi hệ thống xác thực nội bộ");
                return StatusCode(503, new { message = ex.Message }); // 503 Service Unavailable
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi đăng nhập");
                return StatusCode(500, new { message = "Có lỗi xảy ra, vui lòng thử lại sau." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmLogout([FromForm] LogoutViewModel model)
        {

            //xóa cache user
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
            var key = $"{AuthCacheOptions.CacheUserInfor}{sub}";
            await _internalCacheService.RemoveAsync(key);
            // 1️⃣ Sign out local cookie
            await HttpContext.SignOutAsync();

            // 2️⃣ Lấy thông tin logout context
            var logoutContext = await _interaction.GetLogoutContextAsync(model.LogoutId);

            // 3️⃣ Redirect về client
            if (!string.IsNullOrEmpty(logoutContext?.PostLogoutRedirectUri))
            {
                return Redirect(logoutContext.PostLogoutRedirectUri);
            }

            return RedirectToAction("LoggedOut");
        }
    }
}


