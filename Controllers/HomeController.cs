using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index(string search, string? categoryName, string? venueName, string sortOrder, int page = 1)
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Category)  // Correct way to include the full EventCategory object
                .Include(e => e.Venue)          // Include the Venue
                .OrderBy(e => e.Date)          // Default sort by Date
                .AsQueryable();

            // Filter by Title
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Title.Contains(search));

            // Filter by Category
            if (!string.IsNullOrEmpty(categoryName))
                query = query.Where(e => e.Category.Name == categoryName); // Corrected reference to EventCategory.Name

            // Filter by Venue Name
            if (!string.IsNullOrEmpty(venueName))
                query = query.Where(e => e.Venue.Name.Contains(venueName));

            // Sorting
            switch (sortOrder)
            {
                case "title_desc":
                    query = query.OrderByDescending(e => e.Title);
                    break;
                case "date_asc":
                    query = query.OrderBy(e => e.Date);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(e => e.Date);
                    break;
                default:
                    query = query.OrderBy(e => e.Date); // Default sort by Date
                    break;
            }

            // Calculate pagination
            var total = await query.CountAsync();
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)System.Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;
            ViewData["CategoryName"] = categoryName;
            ViewData["VenueName"] = venueName;
            ViewData["SortOrder"] = sortOrder;

            // Fetch paginated data
            var events = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    CategoryName = e.Category.Name, // Corrected to reference EventCategory.Name
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
