using Duende.IdentityServer.Validation;
using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class CustomTokenRequestValidator : ICustomTokenRequestValidator
    {
        private readonly IDistributedCache _cache;

        // Inject Redis Cache vào đây
        public CustomTokenRequestValidator(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            var request = context.Result.ValidatedRequest;

            // Lấy thông tin user gửi từ lúc Login (SignInResponseDto)
            var jsonPayload = request.Raw.Get(ClaimCustom.custom_user_payload.ToString());

            if (!string.IsNullOrEmpty(jsonPayload))
            {
                // Kiểm tra xem dữ liệu user có hợp lệ không
                var user = JsonSerializer.Deserialize<SignInResponseDto>(jsonPayload);

                if (user == null)
                {
                    context.Result.IsError = true;
                    context.Result.Error = "invalid_user_payload";
                    return;
                }

                // --- QUAN TRỌNG: KHÔNG ADD ROLE/EMAIL TẠI ĐÂY ---
                // Chúng ta chỉ giữ lại payload này để ProfileService tí nữa bốc ra dùng.
                // IdentityServer sẽ tự động ánh xạ Subject ID từ quá trình Authentication trước đó.
            }
        }
    }
}
