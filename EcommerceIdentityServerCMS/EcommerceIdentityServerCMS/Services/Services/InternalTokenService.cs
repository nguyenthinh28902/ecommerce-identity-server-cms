using Duende.IdentityModel.Client;
using EcommerceIdentityServerCMS.Models;
using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using EcommerceIdentityServerCMS.Models.Settings;
using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class InternalTokenService : IInternalTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly InternalAuthOptions _authServiceConfigs;
        private readonly ILogger<InternalTokenService> _logger;
        private readonly IInternalCacheService _internalCacheService;

        public InternalTokenService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<InternalTokenService> logger,
            IOptions<InternalAuthOptions> options,
            IInternalCacheService internalCacheService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _authServiceConfigs = options.Value;
            _internalCacheService = internalCacheService;
        }

        /// <summary>
        /// hàm get token nội bộ
        /// </summary>
        /// <returns></returns>
        public async Task<TokenResponseDto?> GetSystemTokenAsync()
        {
            // 1. Định nghĩa key (Service sẽ tự thêm prefix "InternalAuth:" để tránh trùng với Gateway)
            string cacheKey = $"{AuthCacheOptions.Token_user_internal}{_authServiceConfigs.ClientId}";

            try
            {
                // 2. Kiểm tra từ Redis (thông qua InternalCacheService)
                var cachedToken = await _internalCacheService.GetAsync<TokenResponseDto>(cacheKey);
                if (cachedToken != null && cachedToken.IsLogged)
                {
                    return cachedToken;
                }

                // 3. Nếu Cache hụt (Miss), mới thực hiện xin Token từ Identity Server
                var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                    Address = _authServiceConfigs.TokenEndpoint,
                    ClientId = _authServiceConfigs.ClientId,
                    ClientSecret = _authServiceConfigs.ClientSecret,
                    Scope = "user.internal"
                });

                if (tokenResponse == null || tokenResponse.IsError)
                {
                    _logger.LogWarning("Token request rejected: {Error} - {Description}",
                        tokenResponse?.Error, tokenResponse?.ErrorDescription);

                    return new TokenResponseDto { IsLogged = false };
                }

                // 4. Khởi tạo Object kết quả
                var token = new TokenResponseDto {
                    AccessToken = tokenResponse.AccessToken ?? string.Empty,
                    ExpiresIn = tokenResponse.ExpiresIn,
                    IsLogged = true
                };

                // 5. Lưu vào Redis thông qua Service
                // Trừ đi 60 giây trừ hao thời gian mạng (Network Latency)
                await _internalCacheService.SetAsync(cacheKey, token, tokenResponse.ExpiresIn - 60);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi xử lý xin token nội bộ cho {ClientId}", _authServiceConfigs.ClientId);
                return new TokenResponseDto { IsLogged = false };
            }
        }
    }
}
