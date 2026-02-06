using Duende.IdentityServer.Services;
using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.ViewModels.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceIdentityServerCMS.Controllers
{
    [Route("auth")]
    public class LoginController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;
        public LoginController(IIdentityServerInteractionService interaction, ILogger<LoginController> logger, IConfiguration configuration)
        {
            _interaction = interaction;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("dang-nhap-he-thong")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            // 1. Kiểm tra xem request này có phải từ Client hợp lệ gửi sang không
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (User.Identity?.IsAuthenticated == true)
            {
                if (context != null)
                {
                    return Redirect(returnUrl);
                }
                return Redirect("~/");
            }

            var signInViewModel = new SignInViewModel {
                ReturnUrl = returnUrl
            };

            return View(signInViewModel);
        }

        [HttpGet("dang-xuat-he-thong")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // Kiểm tra xem yêu cầu đăng xuất có hợp lệ không
            var context = await _interaction.GetLogoutContextAsync(logoutId);

            // Nếu không yêu cầu xác nhận (ShowLogoutPrompt = false), có thể Logout thẳng
            // Nhưng ở đây ta sẽ luôn hiện trang xác nhận cho chuyên nghiệp
            var vm = new LogoutViewModel { LogoutId = logoutId };
            return View(vm);
        }


        [HttpGet("dang-nhap-that-bai")]
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // Lấy thông tin lỗi chi tiết từ Identity Server dựa trên errorId
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;
            }

            return View(vm);
        }
    }
}
