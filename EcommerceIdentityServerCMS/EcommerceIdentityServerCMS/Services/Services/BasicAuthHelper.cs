using System.Text;

namespace EcommerceIdentityServerCMS.Services.Services
{
    public static class BasicAuthHelper
    {
        public static (string? ClientId, string? ClientSecret) GetCredentials(HttpRequest request)
        {
            string authHeader = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return (null, null);

            var base64Content = authHeader.Substring(6).Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(base64Content));
            var parts = credentials.Split(':', 2);

            return parts.Length == 2 ? (parts[0], parts[1]) : (null, null);
        }
    }

}
