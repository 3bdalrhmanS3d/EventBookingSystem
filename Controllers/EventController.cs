using EventBookingSystemV1.Data;
using EventBookingSystemV1.DTOs;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EventBookingSystemV1.Controllers
{
    [Route("events")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class EventController : BaseController
    {
        private readonly ILogger<EventController> _logger;
        public EventController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<EventController> logger) : base(context, env)
        {
            _logger = logger;
        }

        // Admin Events - /events
        [HttpGet("")]
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            _logger.LogInformation("Admin Index called (search={Search}, page={Page})", search, page);

            var query = _context.Events.AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim()}%";
                query = query.Where(e =>
                    EF.Functions.Like(e.Title, pattern) ||
                    EF.Functions.Like(e.Category.Name, pattern) ||
                    EF.Functions.Like(e.Venue.Name, pattern)
                );
            }
            

            var total = await query.CountAsync();
            var events = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    CategoryName = e.Category.Name,
                    Description = e.Description,
                    VenueName = e.Venue.Name,
                    Date = e.Date,
                    Price = e.Price,
                    AvgRate = e.Reviews.Average(x=>x.Id),
                    ImageUrl = e.ImageUrl
                })
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;

            return View("AdminEvents", events); // Return a different view for admin

        }

        // Public Events - /events/all
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> Events(string search, int page = 1)
        {
            var query = _context.Events.AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Title.Contains(search) || e.Category.Name.Contains(search) || e.Venue.Name.Contains(search));

            var total = await query.CountAsync();
            var events = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    CategoryName = e.Category.Name,
                    Description = e.Description,
                    VenueName = e.Venue.Name,
                    Date = e.Date,
                    Price = e.Price,
                    OrganizerEmail = e.OrganizerEmail,
                    OrganizerPhone = e.OrganizerPhone,

                    ImageUrl = e.ImageUrl
                })
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;

            return View("UserEvents", events); // Return a different view for the public

        }

        // GET: /events/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }
        private void PopulateDropdowns(int? selectedCategory = null, int? selectedVenue = null)
        {
            ViewBag.Categories = new SelectList(
                _context.EventCategories.AsNoTracking().OrderBy(c => c.Name),
                "Id", "Name",
                selectedCategory
            );
            ViewBag.Venues = new SelectList(
                _context.Venues.AsNoTracking().OrderBy(v => v.Name),
                "Id", "Name",
                selectedVenue
            );
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventDto dto)
        {

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(dto.EventCategoryId, dto.VenueId);
                ModelState.AddModelError("", "Please fill all required fields.");
                TempData["ErrorMessage"] = "Failed to add event. Please check all inputs.";
                return View(dto);
            }

            var imageUrl = await HandleImageAsync(dto.Image, "Event");

            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                EventCategoryId = dto.EventCategoryId,
                VenueId = dto.VenueId,
                Date = dto.Date,
                Price = dto.Price,
                ImageUrl = imageUrl,
                OrganizerEmail = dto.OrganizerEmail,
                OrganizerPhone = dto.OrganizerPhone
            };

            _context.Events.Add(newEvent);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving Event {EventId}", newEvent.Id);
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return View(dto);
            }


            TempData["SuccessMessage"] = "Event added successfully!";
            await LogAuditAsync(nameof(Event), newEvent.Id, "Create", new { newEvent.Title, newEvent.Date, newEvent.Price });

            return RedirectToAction(nameof(Details), new { id = newEvent.Id });

        }


        // GET: /events/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return RedirectToAction(nameof(Index));

            // determine current user (or zero if unauthenticated)
            var userId = CurrentUserId;

            // check favorite
            var isFav = userId == 0
                ? false
                : await _context.Favorites
                    .AnyAsync(f => f.UserId == userId && f.EventId == id);

            // Get all reviews for the event
            var reviews = await _context.Reviews
                                  .Where(r => r.EventId == id)
                                  .ToListAsync();

            // Safely calculate average rating
            var averageRating = reviews.Any()
                ? reviews.Average(r => r.Rating)
                : 0;

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
                OrganizerEmail = ev.OrganizerEmail,
                OrganizerPhone = ev.OrganizerPhone,
                Bookings = await _context.Bookings
                                  .Where(b => b.EventId == id)
                                  .Select(b => new BookingInfoViewModel { /*…*/ })
                                  .ToListAsync(),
                Reviews = reviews.Select(r => new ReviewInfoViewModel { /*…*/ }).ToList(),
                AverageRating = averageRating,
                IsFavorited = isFav
            };

            return View(vm);
        }

        // GET: /events/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {

            var eventToEdit = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventToEdit == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            var eventDto = new EventViewModel
            {
                Id = eventToEdit.Id,
                Title = eventToEdit.Title,
                Description = eventToEdit.Description,
                CategoryName = eventToEdit.Category.Name,  
                VenueName = eventToEdit.Venue.Name,        
                Date = eventToEdit.Date,
                Price = eventToEdit.Price
            };

            return View(eventDto);
        }

        // POST: /events/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventDto dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please fill all required fields.");
                TempData["ErrorMessage"] = "Failed to add event. Please check all inputs.";
                return View(dto);
            }

            var eventToUpdate = await _context.Events.FindAsync(id);
            if (eventToUpdate == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update event details
            eventToUpdate.Title = dto.Title;
            eventToUpdate.Description = dto.Description;
            eventToUpdate.EventCategoryId = dto.EventCategoryId;
            eventToUpdate.VenueId = dto.VenueId;
            eventToUpdate.Date = dto.Date;
            eventToUpdate.Price = dto.Price;

            // Handle image upload and delete the old image if a new one is uploaded
            eventToUpdate.ImageUrl = await HandleImageAsync(dto.Image, "Event", eventToUpdate.ImageUrl);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event updated successfully!";
            await LogAuditAsync(nameof(Event), eventToUpdate.Id, "Update", new { eventToUpdate.Title, eventToUpdate.Date, eventToUpdate.Price });

            return RedirectToAction(nameof(Details), new { id = eventToUpdate.Id });
        }

        // POST: /events/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();

            await LogAuditAsync(nameof(Event), eventToDelete.Id, "Delete", new { eventToDelete.Title, eventToDelete.Date, eventToDelete.Price });

            TempData["SuccessMessage"] = "Event deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        
    }
}
