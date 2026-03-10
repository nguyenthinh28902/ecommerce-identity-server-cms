# Ecommerce Identity server

## Giới thiệu

Hệ thống quản lý danh tính và cấp phép (Identity Provider) tập trung, xây dựng trên nền tảng **Duende IdentityServer**.

## 🛠 Công nghệ sử dụng
- **Framework:** .NET 8 / Duende IdentityServer
- **Database:** SQL Server (Entity Framework Core)
- **Giao thức:** OpenID Connect (OIDC) & OAuth
- **Khác:** Redis cache (chạy bằng Docker)
---

## 🔄 Workflow (Luồng xác thực)
### Cấu hình xác thực tại Web
[Xem tiếp]()

### Xác thực tại identity
1. **Xác thực Client:** Người dùng từ Web/CMS được điều hướng đến Identity Server để thực hiện Login.
- Identity tự tạo token của riêng nó để gọi sang service để xác thực thông tin.
  [Cấu hình Identity](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/tree/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Common/Helpers/Identity/Config)
  ```csharp
   new Client
     {
         ClientId = "IdentityServer",
         ClientSecrets = { new Secret("IdentityServer-secret".Sha256()) },
         AllowedGrantTypes = GrantTypes.ClientCredentials,
         ClientClaimsPrefix = "",
         AllowedScopes =
         {
             // Chỉ add các quyền "Internal" để Gateway có quyền quản trị cao nhất khi gọi Service
             "customer.internal",
         },
         AccessTokenLifetime = 5 * 60 // ⏱️ 5 phút là quá đủ
     }
  ```
2. **Cấp Token:** 
- Sau khi xác thực thành công, Identity Server nhận tại các thông tin người đăng nhập.
  + Lưu thông tin user vào redis
  + Tạo token chỉ chứa thông tin userId (sub).
    
  Đăng nhập phiên làm việc cho identity [AuthService.cs](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Services/Services/AuthService.cs) 
```csharp
   /// <summary>
   /// Thực hiện lưu trữ phiên đăng nhập vào IdentityServer Cookie.
   /// Thiết lập các Claims quan trọng như sub, email, wid và roles.
   /// </summary>
   /// <param name="user">Dữ liệu người dùng từ hệ thống nội bộ.</param>
   public async Task EstablishUserSessionAsync(SignInResponseDto user)
   {
       var claims = new List<Claim>
       {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
        };
      
             // Đóng gói đầy đủ Roles và các thông tin quan trọng để Gateway thực hiện phân quyền
             var userCache = new UserCacheModel {
                 Id = user.Id,
                 Email = user.Email,
                 Roles = user.Roles,
                 Scopes = user.Scopes,
                 WorkplaceId = user.WorkplaceId,
                 WorkplaceType = user.WorkplaceType
             };
      
             var cacheKey = $"{AuthCacheOptions.CacheUserInfor}{user.Id}";
             var expireTime = TimeSpan.FromHours((int)ExpireTimeSpanSignIn.Medium);
      
             // Đồng bộ thông tin vào Redis cho các service khác dùng chung
             await _internalCacheService.SetAsync(cacheKey, userCache, (int)expireTime.TotalSeconds);
      
             var isUser = new IdentityServerUser(user.Id.ToString()) {
                 DisplayName = user.Id.ToString(),
                 AdditionalClaims = claims
             };
      
             await _httpContextAccessor.HttpContext.SignInAsync(isUser);
         }
```
Giảm bớt thông tin user trong token. [GatewayUserProfileService.cs](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Services/Services/GatewayUserProfileService.cs)
```csharp
  public class GatewayUserProfileService : IProfileService
  {
      private readonly ILogger<GatewayUserProfileService> _logger;

      public GatewayUserProfileService(ILogger<GatewayUserProfileService> logger)
      {
          _logger = logger;
      }

      public async Task GetProfileDataAsync(ProfileDataRequestContext context)
      {
          // 1. Lấy thông tin User từ Database/Principal
          var sub = context.Subject.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
          if (string.IsNullOrEmpty(sub)) return;

          context.IssuedClaims = new List<Claim>
           {
                  new Claim(JwtRegisteredClaimNames.Sub, sub)
           };
      }
  }
```
### Cấu hình xác thực tại Getaway 
[Xem tiếp]().
### Xác thực tại Service (Product servcie)
[Xem tiếp]().

## Cấu hình khác của identity
- Xử lý lấy token nội bộ cho Identity. [InternalTokenService](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Services/Services/InternalTokenService.cs). 
- Cấu hình identity serivce. [AuthenticationIdentityServer.cs](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Common/Helpers/Identity/AuthenticationIdentityServer.cs)
```csharp
   services.AddStackExchangeRedisCache(options =>
   {
       options.Configuration = RedisConnectionString;
       options.InstanceName = InstanceName;
   });
  
   services.AddIdentityServer(options =>
   {
       // --- CẤU HÌNH ĐƯỜNG DẪN LOGIN TẠI ĐÂY ---
     
   })
   // 2. Cấu hình Database cho Configuration Store (Clients, Resources, Scopes)
   .AddConfigurationStore(options =>
   {
       options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
           sql => sql.MigrationsAssembly(migrationsAssembly));
   })
   // 3. Cấu hình Database cho Operational Store (Tokens, Codes, Consents)
   .AddOperationalStore(options =>
   {
       options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
           sql => sql.MigrationsAssembly(migrationsAssembly));
       // Tự động dọn dẹp các token đã hết hạn trong DB
       options.EnableTokenCleanup = true;
       options.TokenCleanupInterval = 3600; // 1 giờ dọn 1 lần
   })
   .AddDeveloperSigningCredential()
   .AddProfileService<GatewayUserProfileService>();
```

- Cấu hình xác thực cookie. [AuthenticationExtensions.cs](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms/blob/main/EcommerceIdentityServerCMS/EcommerceIdentityServerCMS/Common/Helpers/AddAuthenticationExtensions.cs)
```csharp
    services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "identity_auth_session";
        // Đổi về Lax vì đi qua Gateway/Cùng Domain chính sẽ an toàn và dễ chịu hơn
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(hours);
        options.SlidingExpiration = true;
    });
```
