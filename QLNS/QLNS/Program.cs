using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QLNS.Models;
using QLNS.Services;
using QLNS.Helpers;

var builder = WebApplication.CreateBuilder(args);

// MVC


builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new TimeOnlyConverter());
    });
// DbContext
builder.Services.AddDbContext<FaceIdHrmsContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FaceID_HRMS")));

// AUTHENTICATION (FACEID)
builder.Services.AddAuthentication("FaceIDAuth")
    .AddCookie("FaceIDAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddHttpClient();

// đăng ký services log
builder.Services.AddScoped<LogService>();

// AUTHORIZATION
builder.Services.AddAuthorization();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠ BẮT BUỘC theo thứ tự
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
