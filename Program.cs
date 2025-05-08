using EventBookingSystemV1.Data;
using EventBookingSystemV1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystemV1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
 
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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

                bool anyAdmin = context.Users.Any(u => u.Role == UserRole.Admin);
                if (!anyAdmin)
                {
                    var defaultAdmin = new User
                    {
                        FullName = adminName,
                        Email = adminEmail,
                        Role = UserRole.Admin,
                        IsActive = true,
                        BirthDate = DateTime  .UtcNow,
                        CreatedAt = DateTime  .UtcNow
                    };

                    defaultAdmin.PasswordHash = hasher.HashPassword(defaultAdmin, adminPass);

                    context.Users.Add(defaultAdmin);
                    context.SaveChanges();

                }
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
