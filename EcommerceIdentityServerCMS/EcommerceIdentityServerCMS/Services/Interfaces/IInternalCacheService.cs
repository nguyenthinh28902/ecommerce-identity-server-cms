namespace EcommerceIdentityServerCMS.Services.Interfaces
{
    public interface IInternalCacheService
    {
        Task SetAsync<T>(string key, T value, int expirationMinutes) where T : class;
        Task<T?> GetAsync<T>(string key) where T : class;
        Task RemoveAsync(string key);
    }
}
