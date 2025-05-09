using System;
using EventBookingSystemV1.Configuration;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Password hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 3) JWT settings & service (for issuing your JWTs, if still needed)
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JWT"));
builder.Services.AddScoped<IJwtService, JwtService>();

// 4) Email queue using MailKit
builder.Services.AddSingleton<EmailQueueService>();
builder.Services.AddHostedService<EmailQueueBackgroundService>();

// 5) Authentication & Cookie scheme
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/account/signin";
        options.AccessDeniedPath = "/account/signin";
        options.Cookie.Name = "ebs_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(15);
    });

// 6) MVC support
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 7) Migrate & seed default admin
using (var scope = app.Services.CreateScope())
{
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    context.Database.Migrate();

    var adminConfig = cfg.GetSection("DefaultAdmin");
    var email = adminConfig["Email"];
    var name = adminConfig["FullName"];
    var pass = adminConfig["Password"];

    if (!context.Users.Any(u => u.Role == UserRole.Admin))
    {
        var admin = new User
        {
            FullName = name,
            Email = email,
            Role = UserRole.Admin,
            IsActive = true,
            BirthDate = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, pass);
        context.Users.Add(admin);
        context.SaveChanges();
    }
}

// 8) Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ** Authentication must come before Authorization **
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
