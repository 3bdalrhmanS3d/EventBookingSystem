using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Logging;

namespace EventBookingSystemV1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private const int PageSize = 8;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index(
            string search,
            string? categoryName,
            string? venueName,
            string sortOrder,
            int page = 1)
        {
            var searchPattern = $"%{search?.Trim()}%";

            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Where(e =>
                    (string.IsNullOrWhiteSpace(search)
                        || EF.Functions.Like(e.Title, searchPattern))
                    && (string.IsNullOrEmpty(categoryName)
                        || e.Category.Name == categoryName)
                    && (string.IsNullOrEmpty(venueName)
                        || EF.Functions.Like(e.Venue.Name, $"%{venueName.Trim()}%"))
                );

            query = sortOrder switch
            {
                "title_desc" => query.OrderByDescending(e => e.Title),
                "date_asc" => query.OrderBy(e => e.Date),
                "date_desc" => query.OrderByDescending(e => e.Date),
                _ => query.OrderBy(e => e.Date)
            };

            var total = await query.CountAsync();
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;
            ViewData["CategoryName"] = categoryName;
            ViewData["VenueName"] = venueName;
            ViewData["SortOrder"] = sortOrder;

            var events = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    CategoryName = e.Category.Name,
                    VenueName = e.Venue.Name,
                    Date = e.Date,
                    Price = e.Price,
                    ImageUrl = e.ImageUrl
                })
                .ToListAsync();

            return View(events);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
