using Microsoft.AspNetCore.Mvc;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using EventBookingSystemV1.DTOs;
using EventBookingSystemV1.ViewModels;
namespace EventBookingSystemV1.Controllers
{
    [Route("venues")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class VenueController : BaseController
    {
        public VenueController(ApplicationDbContext context, IWebHostEnvironment env) : base(context, env)
        {

        }

        // Venues
        // GET: /venues
        [HttpGet("")]
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var query = _context.Venues.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(v => v.Name.Contains(search) || v.Address.Contains(search));

            var total = await query.CountAsync();
            var venues = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(v => new VenueViewModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    Address = v.Address,
                    Capacity = v.Capacity,
                    ContactInfo = v.ContactInfo
                })
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;

            return View(venues);
        }
        // GET: /venues/create
        [HttpGet("create")]
        public IActionResult CreateVenue()
        {
            return View(new VenueDto());
        }

        // POST: /venues/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenue(VenueDto dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please fill all required fields.");
                return View(dto);
            }

            var VenueExists = await _context.Venues.AnyAsync(v => v.Name.ToLower().Trim() == dto.Name.ToLower().Trim());
            if (VenueExists)
            {
                ModelState.AddModelError("", "Venue with this name already exists.");
                return View(dto);
            }

            var newVenue = new Venue
            {
                Name = dto.Name,
                Address = dto.Address,
                Capacity = dto.Capacity,
                ContactInfo = dto.ContactInfo
            };

            _context.Venues.Add(newVenue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue added successfully!";
            await LogAuditAsync(nameof(Venue), newVenue.Id , "Create", new { dto.Name, dto.Address, dto.Capacity });
            return RedirectToAction(nameof(Index));
        }
        // GET: /venues/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> EditVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                TempData["ErrorMessage"] = "Venue not found.";
                return RedirectToAction(nameof(Index));
            }
            // Manual map from entity to DTO
            var dto = new VenueDto
            {
               
                Name = venue.Name,
                Address = venue.Address,
                Capacity = venue.Capacity,
                ContactInfo = venue.ContactInfo
            };

            return View(dto);
        }

        // POST: /venues/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVenue(int id, VenueDto dto)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please fill all required fields.");
                return View(dto);
            }

            var venueToUpdate = await _context.Venues.FindAsync(id);
            if (venueToUpdate == null)
            {
                TempData["ErrorMessage"] = "Venue not found.";
                return RedirectToAction(nameof(Index));
            }

            venueToUpdate.Name = dto.Name;
            venueToUpdate.Address = dto.Address;
            venueToUpdate.Capacity = dto.Capacity;
            venueToUpdate.ContactInfo = dto.ContactInfo;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue updated successfully!";
            await LogAuditAsync(nameof(Venue), venueToUpdate.Id, "Update", new { venueToUpdate.Name, venueToUpdate.Address, venueToUpdate.Capacity });
            return RedirectToAction(nameof(Index));
        }
        // POST: /venues/delete/{id}
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venueToDelete = await _context.Venues.FindAsync(id);
            if (venueToDelete == null)
            {
                TempData["ErrorMessage"] = "Venue not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venues.Remove(venueToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue deleted successfully!";
            await LogAuditAsync(nameof(Venue), venueToDelete.Id, "Delete", new { venueToDelete.Name, venueToDelete.Address, venueToDelete.Capacity });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var venue = await _context.Venues
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue == null) return NotFound();

            var vm = new VenueViewModel
            {
                Id = venue.Id,
                Name = venue.Name,
                Address = venue.Address,
                Capacity = venue.Capacity,
                ContactInfo = venue.ContactInfo
            };

            return View(vm);
        }

    }
}
