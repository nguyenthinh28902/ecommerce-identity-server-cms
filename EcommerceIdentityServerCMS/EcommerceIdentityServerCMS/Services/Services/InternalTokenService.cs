using EcommerceIdentityServerCMS.Common.Exceptions;
using EcommerceIdentityServerCMS.Models.DTOs.SignIn;
using EcommerceIdentityServerCMS.Models.Settings;
using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class InternalTokenService : IInternalTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ServiceAuthOptions _configs;
        private readonly ILogger<InternalTokenService> _logger;

        public InternalTokenService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<InternalTokenService> logger,
            IOptions<ServiceAuthOptions> options)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _configs = options.Value;
        }

        // =========================
        // PUBLIC API
        // =========================

        public Task<TokenResponseDto?> GetSystemTokenAsync()
        {
            return RequestTokenAsync();
        }

        public async Task<TokenResponseDto?> ExchangeAuthorizationCodeAsync(
            ServiceAuthOptions serviceAuthOptions,
            ExchangeRequest exchangeRequest
            )
        {
            if (string.IsNullOrWhiteSpace(exchangeRequest.Code))
                throw new UnauthorizedException("Thiếu authorization_code");

            var form = new Dictionary<string, string> {
                ["grant_type"] = "authorization_code",
                ["client_id"] = serviceAuthOptions.ClientId,
                ["client_secret"] = serviceAuthOptions.ClientSecret,
                ["code"] = exchangeRequest.Code,
                ["code_verifier"] = exchangeRequest.CodeVerifier,
                ["redirect_uri"] = exchangeRequest.RedirectUri
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(
                    _configuration["InternalAuth:TokenEndpoint"],
                    new FormUrlEncodedContent(form)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to exchange authorization_code for service {Service}",
                    serviceAuthOptions.ClientId);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Token endpoint rejected code for {Service}: {Error}",
                    serviceAuthOptions.ClientId,
                    error);

                throw new UnauthorizedException("authorization_code không hợp lệ hoặc đã hết hạn");
            }

            var json = await response.Content.ReadAsStringAsync();

            var token = JsonSerializer.Deserialize<TokenResponseDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (!string.IsNullOrEmpty(token?.AccessToken))
                token.IsLogged = true;

            return token;
        }


        // =========================
        // CORE
        // =========================

        private async Task<TokenResponseDto?> RequestTokenAsync()
        {
            var form = BuildTokenRequestForm(_configs);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(
                    _configuration["InternalAuth:TokenEndpoint"],
                    new FormUrlEncodedContent(form));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token request failed for service {Service}", _configs.ClientId);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Token endpoint rejected request for {Service}: {Error}",
                    _configs.ClientId,
                    error);

                throw new ForbiddenException("Không có quyền truy cập service này");
            }

            var content = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<TokenResponseDto>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!string.IsNullOrEmpty(token?.AccessToken))
                token.IsLogged = true;

            return token;
        }

        // =========================
        // HELPERS
        // =========================

        private static Dictionary<string, string> BuildTokenRequestForm(
         ServiceAuthOptions cfg)
        {
            var form = new Dictionary<string, string> {
                ["grant_type"] = cfg.GrantType,
                ["client_id"] = cfg.ClientId,
                ["client_secret"] = cfg.ClientSecret
            };
            if (!string.IsNullOrEmpty(cfg.Scope))
                form["scope"] = cfg.Scope;
            return form;
        }
    }
}
