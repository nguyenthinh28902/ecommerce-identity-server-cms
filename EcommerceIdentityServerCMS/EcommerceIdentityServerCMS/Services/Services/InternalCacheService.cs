using EcommerceIdentityServerCMS.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public class InternalCacheService : IInternalCacheService
    {
        private readonly IDistributedCache _cache;
        // Đây là "vùng tên" riêng cho Identity để không lẫn với UserSession của Gateway
        private const string IDENTITY_INTERNAL_PREFIX = "InternalAuth:";

        public InternalCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Lưu dữ liệu vào Cache
        public async Task SetAsync<T>(string key, T value, int expirationSeconds) where T : class
        {
            var cacheKey = $"{IDENTITY_INTERNAL_PREFIX}{key}";

            var options = new DistributedCacheEntryOptions {
                // Hết hạn tuyệt đối (thường trừ đi 60s để an toàn cho Token)
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(cacheKey, jsonData, options);
        }

        // Lấy dữ liệu từ Cache
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var cacheKey = $"{IDENTITY_INTERNAL_PREFIX}{key}";
            var jsonData = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(jsonData))
                return null;

            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync($"{IDENTITY_INTERNAL_PREFIX}{key}");
        }
    }
}
