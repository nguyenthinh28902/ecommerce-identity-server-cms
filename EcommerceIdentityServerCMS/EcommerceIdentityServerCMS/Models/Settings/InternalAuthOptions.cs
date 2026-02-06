namespace EcommerceIdentityServerCMS.Models.Settings
{
    public class InternalAuthOptions
    {
        // ClientId đã đăng ký trong IdentityServer (Database)
        public string TokenEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;

        // Secret tương ứng để Gateway xác thực với IdentityServer
        public string ClientSecret { get; set; } = string.Empty;

        // Các quyền mà Service này yêu cầu (ví dụ: "openid profile personnel.read")
        public string Scopes { get; set; } = string.Empty;

        public string GrantType { get; set; } = string.Empty;
    }
}
