using EcommerceIdentityServerCMS.Common.Exceptions;
using EcommerceIdentityServerCMS.Common.Helpers;
using EcommerceIdentityServerCMS.Common.Helpers.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// trang signin
builder.Services.AddControllersWithViews();

//settings config
builder.Services.AddConfigAppSetting(builder.Configuration);
//identity server
builder.Services.AddAuthenticationIdentityServer(builder.Configuration);
builder.Services.AddAuthenticationExtensions(builder.Configuration);
builder.Services.AddServiceDI(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles(); // Thêm dòng này nếu có giao diện Login UI
app.UseCookiePolicy();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();
// Gọi Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    await DatabaseInitializer.InitDatabaseAsync(services, config);
}
app.Run();
