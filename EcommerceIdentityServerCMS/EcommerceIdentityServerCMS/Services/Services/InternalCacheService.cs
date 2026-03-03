using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class InternalCacheService : IInternalCacheService
    {
        private readonly ILogger<InternalCacheService> _logger;
        private readonly IDistributedCache _cache;
        // Đây là "vùng tên" riêng cho Identity để không lẫn với UserSession của Gateway
        private const string IDENTITY_INTERNAL_PREFIX = "InternalAuth:";

        public InternalCacheService(IDistributedCache cache, ILogger<InternalCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Lưu dữ liệu vào Cache với thời gian hết hạn (thường dùng cho Token)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">get set cache</param>
        /// <param name="value">dữ liệu set</param>
        /// <param name="expirationSeconds">thời gian set</param>
        /// <returns></returns>
        public async Task SetAsync<T>(string key, T value, int expirationSeconds) where T : class
        {
            try
            {
                var cacheKey = $"{IDENTITY_INTERNAL_PREFIX}{key}";

                var options = new DistributedCacheEntryOptions
                {
                    // Hết hạn tuyệt đối (thường trừ đi 60s để an toàn cho Token)
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
                };

                var jsonData = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(cacheKey, jsonData, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key {Key}", key);
            }
        }

        // Lấy dữ liệu từ Cache
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cacheKey = $"{IDENTITY_INTERNAL_PREFIX}{key}";
                var jsonData = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(jsonData))
                    return null;

                return JsonSerializer.Deserialize<T>(jsonData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key {Key}", key);
                return null;
            }
        }

        // xóa cache
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync($"{IDENTITY_INTERNAL_PREFIX}{key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key {Key}", key);
            }
        }
    }
}
