using EventBookingSystemV1.Configuration;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Identity helpers
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// 3) Bind JWT settings and register JWT service
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JWT"));
builder.Services.AddScoped<IJwtService, JwtService>();

// 4) Register Email queue services using MailKit
builder.Services.AddSingleton<EmailQueueService>();
builder.Services.AddHostedService<EmailQueueBackgroundService>();

// 5) Add MVC support
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Apply migrations and seed default admin
using (var scope = app.Services.CreateScope())
{
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    context.Database.Migrate();

    var adminConfig = cfg.GetSection("DefaultAdmin");
    string adminEmail = adminConfig["Email"];
    string adminName = adminConfig["FullName"];
    string adminPass = adminConfig["Password"];

    if (!context.Users.Any(u => u.Role == UserRole.Admin))
    {
        var defaultAdmin = new User
        {
            FullName = adminName,
            Email = adminEmail,
            Role = UserRole.Admin,
            IsActive = true,
            BirthDate = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        defaultAdmin.PasswordHash = hasher.HashPassword(defaultAdmin, adminPass);
        context.Users.Add(defaultAdmin);
        context.SaveChanges();
    }
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
