using Duende.IdentityServer.Models;

namespace EcommerceIdentityServerCMS.Common.Helpers.Identity.Config
{
    public static class ApiScopes
    {
        public static IEnumerable<ApiScope> Get()
        {
            return new[]
            {
                // --- PRODUCT SERVICE ---
                new ApiScope("product.internal", "Full access to Product Service"),
                new ApiScope("product.read", "Read product information"),
                new ApiScope("product.write", "Create/Update/Delete products"),

                // --- ORDER SERVICE ---
                new ApiScope("order.internal", "Full access to Order Service"),
                new ApiScope("order.read", "View order history"),
                new ApiScope("order.write", "Place or Modify orders"),

                // --- STOCK SERVICE ---
                new ApiScope("stock.internal", "Full access to Stock Service"),
                new ApiScope("stock.read", "Check inventory levels"),
                new ApiScope("stock.write", "Update stock quantities"),

                // --- PAYMENT SERVICE ---
                new ApiScope("payment.internal", "Full access to Payment Service"),
                new ApiScope("payment.read", "View payment transactions"),
                new ApiScope("payment.write", "Process payments or refunds"),

                // user
                new ApiScope("user.internal", "User full access"),
                new ApiScope("user.read", "User read information"),
                new ApiScope("user.write", "User write information"),
                // Customer
                new ApiScope("customer.internal", "customer full access"),
                new ApiScope("customer.read", "customer read information"),
                new ApiScope("customer.write", "customer write information"),
            };
        }
    }
}
