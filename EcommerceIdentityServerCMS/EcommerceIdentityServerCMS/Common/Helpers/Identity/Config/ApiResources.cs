using Duende.IdentityServer.Models;

namespace EcommerceIdentityServerCMS.Common.Helpers.Identity.Config
{
    public static class ApiResources
    {
        public static IEnumerable<ApiResource> Get()
        {
            return new[]
            {
                    // Resource mới dành riêng cho Customer Service
                    // 1. Identity/User Service Resource
                new ApiResource("user.api", "Identity CMS Service API")
                {
                    Scopes = { "user.internal", "user.read", "user.write" },
                    // UserClaims giúp đính kèm thêm thông tin vào Access Token khi gọi API này
                    UserClaims = { "sub" },
                },
                new ApiResource("product.api", "Product Service API")
                {
                    Scopes = { "product.internal", "product.read", "product.write" },
                    // UserClaims giúp đính kèm thêm thông tin vào Access Token khi gọi API này
                    UserClaims = { "sub" },
                },
                new ApiResource("order.api", "Order Service API")
                {
                    Scopes = { "order.internal", "order.read", "order.write" },
                    // UserClaims giúp đính kèm thêm thông tin vào Access Token khi gọi API này
                    UserClaims = { "sub" },
                },
                new ApiResource("payment.api", "Payment Service API")
                {
                    Scopes = { "payment.internal", "payment.read", "payment.write" },
                    // UserClaims giúp đính kèm thêm thông tin vào Access Token khi gọi API này
                    UserClaims = { "sub" },
                }
              };
        }
    }
}
