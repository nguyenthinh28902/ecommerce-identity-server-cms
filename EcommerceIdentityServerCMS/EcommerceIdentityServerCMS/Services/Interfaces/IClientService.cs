namespace EcommerceIdentityServerCMS.Services.Interfaces
{
    public interface IClientService
    {
        Task<string> GetAllowedScopesAsync(string clientId);
    }
}
