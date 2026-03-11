# Enterprise Identity Provider (IAM) - Trung tâm định danh hệ thống

## 📝 Giới thiệu
Hệ thống quản lý danh tính và cấp phép tập trung (Identity Provider - IdP), được xây dựng trên nền tảng **Duende IdentityServer**. Dự án đóng vai trò là cửa ngõ bảo mật cốt lõi, quản lý toàn bộ vòng đời xác thực và phân quyền cho hệ sinh thái Microservices.

### 🔗 Core Security & Implementation (Liên kết kỹ thuật trọng tâm)

> **Tổng quan dự án xem tại đây:** [Xem đầy đủ kiến trúc tại đây](https://github.com/nguyenthinh28902/mini-project-ecommerce)

Để đi sâu vào các cấu hình bảo mật hệ thống, bạn có thể tham khảo trực tiếp tại các module sau:

* **Client Security:** Triển khai OIDC Middleware, quản lý Secure Cookie và luồng Challenge.
  * [Cấu hình tại Web CMS](https://github.com/nguyenthinh28902/ecommerce-cms-web)
* **Identity Provider:** Định nghĩa Resource, Scope và Custom Profile Service để mapping Claims.
  * [Cấu hình tại Identity Server](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms)
* **API Gateway (YARP):** Quản lý Reverse Proxy Routing và thiết lập Auth Policy tập trung.
  * [Cấu hình tại Gateway CMS](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms)
* **Resource Server:** Cấu hình JWT Bearer và phân quyền dựa trên Policy (Policy-based Authorization).
  * [Cấu hình tại Product Service](https://github.com/nguyenthinh28902/Ecom.ProductService)
---

## 🛠 Công nghệ & Hạ tầng
- **Core Framework:** .NET 8 / Duende IdentityServer.
- **Security Protocols:** OpenID Connect (OIDC) & OAuth 2.0.
- **Persistence:** SQL Server (Entity Framework Core) cho Configuration & Operational Store.
- **Caching & Session:** Redis Cache (Docker) tối ưu tốc độ truy xuất thông tin người dùng.

---

## 🔐 Technical Implementation (Triển khai kỹ thuật)

### 1. Internal Authentication Mechanism (Xác thực nội bộ)
Identity Server sử dụng cơ chế **Client Credentials** để khởi tạo Token nội bộ, cho phép nó truy vấn an toàn sang các Business Service khác nhằm verify thông tin người dùng.

* **Cấu hình chi tiết:** [Identity Config Helpers](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/tree/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Common/Helpers/Identity/Config)
* **Giải pháp:** Thiết lập các `AllowedScopes` mức "Internal" (`customer.internal`) với vòng đời Token ngắn (5 phút) để tối đa hóa tính bảo mật.

```csharp
new Client {
    ClientId = "IdentityServer",
    ClientSecrets = { new Secret("IdentityServer-secret".Sha256()) },
    AllowedGrantTypes = GrantTypes.ClientCredentials,
    AllowedScopes = { "customer.internal" },
    AccessTokenLifetime = 300 // 5 phút
}
```

### 2. Session Management & Token Optimization (Quản lý phiên & Tối ưu Token)
Triển khai cơ chế lưu trữ tập trung tại Redis và tối ưu hóa kích thước JWT để giảm tải băng thông cho Gateway.

* **File xử lý:** [AuthService.cs](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Services/Services/AuthService.cs) & [GatewayUserProfileService.cs](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Services/Services/GatewayUserProfileService.cs)
* **Giải pháp:** * Toàn bộ Claims chi tiết (Roles, Scopes, Workplace) được lưu vào Redis.
    * Access Token trả về Client chỉ chứa thông tin định danh tối giản (`sub`), buộc Gateway phải tham chiếu vào Cache để thực hiện phân quyền.

```csharp
// Đăng nhập phiên làm việc và đồng bộ dữ liệu vào Redis
public async Task EstablishUserSessionAsync(SignInResponseDto user) {
    var userCache = new UserCacheModel { Id = user.Id, Roles = user.Roles, Scopes = user.Scopes };
    var cacheKey = $"{AuthCacheOptions.CacheUserInfor}{user.Id}";
    
    // Đồng bộ thông tin vào Redis cho các service khác dùng chung
    await _internalCacheService.SetAsync(cacheKey, userCache, expireSeconds);
    
    var isUser = new IdentityServerUser(user.Id.ToString()) { AdditionalClaims = claims };
    await _httpContextAccessor.HttpContext.SignInAsync(isUser);
}
```

### 3. Identity Infrastructure Configuration (Cấu hình hạ tầng Identity)
Thiết lập bộ lưu trữ Database cho cấu hình hệ thống và quản lý vòng đời Token tự động.

* **File cấu hình:** [AuthenticationIdentityServer.cs](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Common/Helpers/Identity/AuthenticationIdentityServer.cs)
* **Giải pháp:** Sử dụng `AddConfigurationStore` và `AddOperationalStore` để tách biệt dữ liệu cấu hình và dữ liệu vận hành. Kích hoạt `EnableTokenCleanup` để tự động dọn dẹp các mã định danh hết hạn.

```csharp
services.AddIdentityServer()
    .AddConfigurationStore(options => {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString);
    })
    .AddOperationalStore(options => {
        options.ConfigureDbContext = b => b.UseSqlServer(connectionString);
        options.EnableTokenCleanup = true; // Tự động dọn dẹp token hết hạn
    })
    .AddProfileService<GatewayUserProfileService>();
```
---
