using EventBookingSystemV1.Data;
using System.Text.Json;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystemV1.Controllers
{
    [Route("admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminController : BaseController
    {
        public AdminController(ApplicationDbContext context, IWebHostEnvironment env) : base(context, env)
        {

        }

        /// <summary>
        /// Displays the admin dashboard with summary statistics.
        /// </summary>
        // GET /admin/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalEvents = await _context.Events.CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                PendingVerifications = await _context.AccountActivation.CountAsync(a => !a.IsUsed)
            };
            return View(vm);
        }

        /// <summary>
        /// List users with optional search and paging.
        /// </summary>
        // GET /admin/users
        [HttpGet("users")]
        public async Task<IActionResult> Users(string search, int page = 1)
        {
            var query = _context.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var total = await query.CountAsync();
            var raw = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = (UserRoleViewModel)u.Role,
                    BirthDate = u.BirthDate,
                    IsActive = u.IsActive,
                    
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            foreach (var u in raw)
            {
                var age = DateTimeOffset.UtcNow.Year - u.BirthDate.Year;
                if (u.BirthDate > DateTimeOffset.UtcNow.AddYears(-age)) age--;
                u.Age = age;
            }

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;

            return View(raw);
        }

        /// <summary>
        /// Displays user details including booking, review, and favorite counts.
        /// </summary>

        // GET /admin/users/{id}
        [HttpGet("users/{id}")]
        public async Task<IActionResult> UserDetails(int id)
        {
            var data = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.BirthDate,
                    u.IsActive,
                    u.CreatedAt,
                    Bookings = u.Bookings.Select(b => new
                    {
                        b.Id,
                        b.BookedAt,
                        EventTitle = b.Event.Title,
                        EventDate = b.Event.Date
                    }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (data == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var age = DateTimeOffset.UtcNow.Year - data.BirthDate.Year;
            if (data.BirthDate > DateTimeOffset.UtcNow.AddYears(-age)) age--;

            var vm = new UserDetailsViewModel
            {
                Id = data.Id,
                FullName = data.FullName,
                Email = data.Email,
                Role = (UserRoleViewModel)data.Role,
                BirthDate = data.BirthDate,
                Age = age,
                IsActive = data.IsActive,
                CreatedAt = data.CreatedAt,
                Bookings = data.Bookings.Select(b => new BookingViewModel
                {
                    Id = b.Id,
                    EventTitle = b.EventTitle,
                    EventDate = b.EventDate,
                    BookedAt = b.BookedAt
                }).ToList()
            };

            return View(vm);
        }


        /// <summary>
        /// List all events with their category and venue.
        /// </summary>
        // GET /admin/events
        [HttpGet("events")]
        public async Task<IActionResult> Events(string search, int page = 1)
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Venue)
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
                });

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Title.Contains(search) || e.CategoryName.Contains(search) || e.VenueName.Contains(search));

            var total = await query.CountAsync();
            var events = await query
                .OrderBy(e => e.Date)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;

            return View(events);
        }


        // GET /admin/events/{id}
        [HttpGet("events/{id}")]
        public async Task<IActionResult> EventDetails(int id)
        {
            // load event core data first
            var ev = await _context.Events
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction(nameof(Events));
            }

            var bookings = await _context.Bookings
                .Where(b => b.EventId == id)
                .Select(b => new BookingInfoViewModel
                {
                    Id = b.Id,
                    UserName = b.User.FullName,
                    UserEmail = b.User.Email,
                    BookedAt = b.BookedAt,
                    TicketCount = b.Tickets.Count()
                })
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.EventId == id)
                .Select(r => new ReviewInfoViewModel
                {
                    Reviewer = r.User.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var vm = new EventDetailsViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Category = ev.Category.Name,
                Venue = ev.Venue.Name,
                Date = ev.Date,
                Price = ev.Price,
                ImageUrl = ev.ImageUrl,
                Bookings = bookings,
                Reviews = reviews
            };

            return View(vm);
        }


        /// <summary>
        /// Delete an event by id.
        /// </summary>
        // POST /admin/events/{id}/delete
        [HttpPost("events/{id}/delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction(nameof(Events));
            }
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            await LogAuditAsync("Event", id, "Delete", new { ev.Title, ev.Date, ev.Price });
            return RedirectToAction(nameof(Events));
        }


        /// <summary>
        /// List all bookings with user and event details, with search & paging.
        /// </summary>
        // GET /admin/bookings
        [HttpGet("bookings")]
        public async Task<IActionResult> Bookings(string search, int page = 1)
        {
            var query = _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Event)
                .Select(b => new BookingListViewModel
                {
                    Id = b.Id,
                    UserName = b.User.FullName,
                    UserEmail = b.User.Email,
                    EventTitle = b.Event.Title,
                    EventDate = b.Event.Date,
                    BookedAt = b.BookedAt,
                    TicketCount = b.Tickets.Count()
                });

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.UserName.Contains(search) || b.EventTitle.Contains(search));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.BookedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;

            return View(items);
        }

        /// <summary>
        /// Delete a booking by id.
        /// </summary>
        // POST /admin/bookings/{id}/delete
        [HttpPost("bookings/{id}/delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction(nameof(Bookings));
            }
            // capture a summary
            var snapshot = new
            {
                booking.User.Email,
                booking.Event.Title,
                booking.BookedAt,
                TicketCount = booking.Tickets.Count
            };

            _context.Tickets.RemoveRange(booking.Tickets);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            await LogAuditAsync("Booking", id, "Delete", snapshot);

            return RedirectToAction(nameof(Bookings));
        }

        /// <summary>
        /// Delete a user by id.
        /// </summary>
        // POST /admin/users/{id}/delete
        [HttpPost("users/{id}/delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            // capture snapshot of key properties before removal
            var snapshot = new { user.FullName, user.Email, user.Role };

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // log it
            await LogAuditAsync(
                entityName: "User",
                entityId: user.Id,
                action: "Delete",
                changes: snapshot
            );

            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Activate a user account.
        /// </summary>
        // POST /admin/users/{id}/activate
        [HttpPost("users/{id}/activate"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }
            user.IsActive = true;
            await _context.SaveChangesAsync();

            await LogAuditAsync("User", id, "Activate", new { user.Email });

            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Deactivate a user account.
        /// </summary>
        // POST /admin/users/{id}/deactivate
        [HttpPost("users/{id}/deactivate"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }
            user.IsActive = false;
            await _context.SaveChangesAsync();

            await LogAuditAsync("User", id, "Deactivate", new { user.Email });

            return RedirectToAction(nameof(Users));
        }

        // GET /admin/audit
        [HttpGet("audit")]
        public async Task<IActionResult> Audit(int page = 1)
        {
            var totalRecords = await _context.AuditLogs.CountAsync();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            var logs = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.PerformedAt) // Order by the performed time
                .Skip((page - 1) * PageSize) // Skip the records from previous pages
                .Take(PageSize) // Take only the records for the current page
                .Select(a => new AuditLogViewModel
                {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Action = a.Action,
                    PerformedBy = a.PerformedBy,
                    PerformedAt = a.PerformedAt,
                })
                .ToListAsync();

            // Pass the data to the View
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(logs);
        }

        // GET /admin/audit/{id}/details
        [HttpGet("audit/{id}/details")]
        public async Task<IActionResult> AuditDetails(int id)
        {
            var log = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (log == null)
                return NotFound();

            var performedByUser = await _context.Users
                .Where(u => u.Email == log.PerformedBy)
                .Select(
                u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    
                })
                .FirstOrDefaultAsync();

            if (performedByUser == null)
                return NotFound();

            var vm = new AuditLogDetailViewModel
            {
                Id = log.Id,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Action = log.Action,
                PerformedBy = log.PerformedBy,
                PerformedAt = log.PerformedAt,
                ChangesJson = log.ChangesJson,
                PerformedByUser = performedByUser
            };

            return View(vm);
        }


    }
}