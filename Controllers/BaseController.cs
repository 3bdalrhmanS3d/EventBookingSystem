using EventBookingSystemV1.Data;
using EventBookingSystemV1.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace EventBookingSystemV1.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IWebHostEnvironment _env;
        protected const int PageSize = 20;

        protected BaseController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        ///Reads the current user number from the Claims
        /// </summary>
        protected int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? throw new InvalidOperationException("User ID claim missing"));
        /// <summary>
        /// Saves an uploaded image to the given folder under wwwroot, deletes old if provided, and returns the URL path.
        /// </summary>
        protected async Task<string> HandleImageAsync(IFormFile file, string folder, string? existingUrl = null)
        {
            if (file == null || file.Length == 0)
                return existingUrl ?? string.Empty;

            // Delete old image
            if (!string.IsNullOrEmpty(existingUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, existingUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Ensure folder exists
            var saveDir = Path.Combine(_env.WebRootPath, folder);
            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            // Save new image
            var fileName = Path.GetFileName(file.FileName);
            var fullPath = Path.Combine(saveDir, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return URL path e.g. /folder/filename.jpg
            return $"/{folder}/{fileName}";
        }

        /// <summary>
        /// Logs an audit record for given entity change.
        /// </summary>
        protected async Task LogAuditAsync(string entityName, int entityId, string action, object changes)
        {
            var username = User.Identity?.Name ?? "System";
            var log = new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                PerformedBy = username,
                PerformedAt = DateTimeOffset.UtcNow,
                ChangesJson = JsonSerializer.Serialize(changes)
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task RefreshUserClaims(User user)
        {
            if (User.Identity is not ClaimsIdentity identity)
                throw new InvalidOperationException("No identity to refresh.");

            var existingName = identity.FindFirst(ClaimTypes.Name);
            var existingDob = identity.FindFirst(ClaimTypes.DateOfBirth);
            if (existingName != null) identity.RemoveClaim(existingName);
            if (existingDob != null) identity.RemoveClaim(existingDob);

            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));
            identity.AddClaim(new Claim(ClaimTypes.DateOfBirth, user.BirthDate.ToString("yyyy-MM-dd")));

            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);
        }


    }
}
