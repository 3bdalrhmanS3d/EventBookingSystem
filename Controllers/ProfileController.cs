using EventBookingSystemV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using EventBookingSystemV1.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EventBookingSystemV1.ViewModels;
using EventBookingSystemV1.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace EventBookingSystemV1.Controllers
{
    [Route("profile")]
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment env, IPasswordHasher<User> passwordHasher)
            : base(context, env)
        {
            _passwordHasher = passwordHasher;
        }

        // GET: /profile
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate
            };

            return View(vm);
        }

        // GET: /profile/edit
        [HttpGet("edit")]
        public async Task<IActionResult> Edit()
        {
            var userId = CurrentUserId;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var dto = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate
            };

            return View(dto);
        }

        // POST: /profile/edit
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = CurrentUserId;
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null) return NotFound();

            // update fields
            user.FullName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}";
            await _context.SaveChangesAsync();

            await RefreshUserClaims(user);

            TempData["SuccessMessage"] = "Profile updated successfully!";

            await LogAuditAsync(nameof(User), user.Id, "UpdateProfile", new { user.FullName, user.BirthDate });

            return RedirectToAction(nameof(Index));
        }


        // GET: /profile/change-password
        [HttpGet("change-password")]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordDto());
        }

        // POST: /profile/change-password
        [HttpPost("change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = CurrentUserId;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var verify = _passwordHasher
                .VerifyHashedPassword(user, user.PasswordHash, dto.OldPassword);
            if (verify != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(nameof(dto.OldPassword), "Current password is incorrect.");
                return View(dto);
            }

            // update to new password
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            await LogAuditAsync(nameof(User), user.Id, "ChangePassword", null);

            return RedirectToAction(nameof(Index));
        }

        // My Bookings
        // GET /profile/bookings
        [HttpGet("bookings")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = CurrentUserId;
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .Include(b => b.Event)
                .OrderByDescending(b => b.BookedAt)
                .Select(b => new BookingViewModel
                {
                    Id = b.Id,
                    EventTitle = b.Event.Title,
                    EventDate = b.Event.Date,
                    BookedAt = b.BookedAt,
                    
                })
                .ToListAsync();

            return View(bookings);
        }

        // POST /profile/bookings/{id}/cancel
        [HttpPost("bookings/{id}/cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == CurrentUserId );

            if (booking == null) return NotFound();

            _context.Tickets.RemoveRange(booking.Tickets);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking canceled successfully.";
            await LogAuditAsync(nameof(Booking), booking.Id, "Cancel", null);
            return RedirectToAction(nameof(Index));
        }

        // GET: /profile/favorites
        [HttpGet("favorites")]
        public async Task<IActionResult> MyFavorites()
        {
            var userId = CurrentUserId;
            var favs = await _context.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Include(f => f.Event)
                .OrderBy(f => f.Event.Title)
                .Select(f => new EventViewModel
                {
                    Id = f.Event.Id,
                    Title = f.Event.Title,
                    CategoryName = f.Event.Category.Name,
                    VenueName = f.Event.Venue.Name,
                    Date = f.Event.Date,
                    Price = f.Event.Price,
                    ImageUrl = f.Event.ImageUrl
                })
                .ToListAsync();

            return View(favs);
        }

        // POST /profile/favorites/toggle/{eventId}
        [HttpPost("favorites/toggle/{eventId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            var userId = CurrentUserId;
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
            }
            else
            {
                _context.Favorites.Add(new Favorite { UserId = userId, EventId = eventId });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyFavorites));
        }

        // (My Notifications)
        // GET: /profile/notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> MyNotifications()
        {
            var userId = CurrentUserId;
            var notes = await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    //Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewData["UnreadCount"] = notes.Count(n => !n.IsRead);
            return View(notes);
        }

        // POST /profile/notifications/{id}/mark-read
        [HttpPost("notifications/{id}/mark-read")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var note = await _context.Notifications.FindAsync(id);
            if (note == null || note.UserId != CurrentUserId)
                return NotFound();

            note.IsRead = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyNotifications));
        }


        //4) My Reviews
        // GET /profile/reviews
        [HttpGet("reviews")]
        public async Task<IActionResult> MyReviews()
        {
            var userId = CurrentUserId;
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    EventTitle = r.Event.Title,
                    EventId = r.Event.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return View(reviews);
        }


        // POST /profile/reviews/create
        [HttpPost("reviews/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(ReviewDto dto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(MyReviews));

            var userId = CurrentUserId;

            // 1) Prevent duplicates:
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.EventId == dto.Id);
            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] = "You have already submitted a review for this event.";
                return RedirectToAction(nameof(MyReviews));
            }

            // 2) Otherwise, create & save
            var review = new Review
            {
                UserId = userId,
                EventId = dto.Id,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Review submitted!";
            return RedirectToAction(nameof(MyReviews));
        }

        // Get Ticket
        // GET: /profile/tickets
        [HttpGet("tickets")]
        public async Task<IActionResult> MyTickets()
        {
            var userId = CurrentUserId;
            var tickets = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.Booking.UserId == userId)
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Event)
                .OrderByDescending(t => t.Booking.BookedAt)
                .Select(t => new MyTicketViewModel
                {
                    Id = t.Id,
                    EventTitle = t.Booking.Event.Title,
                    EventDate = t.Booking.Event.Date,
                    BookingDate = t.Booking.BookedAt,
                    BookingReference = t.Booking.BookingReference,
                    TicketType = t.TicketType,

                })
                .ToListAsync();

            return View(tickets);
        }

        // ProfileController.cs

        // GET /profile/tickets/{id}/print
        [HttpGet("tickets/{id}/print")]
        public async Task<IActionResult> PrintTicket(int id)
        {
            var userId = CurrentUserId;

            var ticket = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.Id == id && t.Booking.UserId == userId)
                .Include(t => t.Booking)
                    .ThenInclude(b => b.Event)
                .Select(t => new MyTicketPrintViewModel
                {
                    TicketId = t.Id,
                    EventTitle = t.Booking.Event.Title,
                    EventDate = t.Booking.Event.Date,
                    VenueName = t.Booking.Event.Venue.Name,
                    BookingDate = t.Booking.BookedAt,
                    BookingReference = t.Booking.BookingReference!,
                    TicketType = t.TicketType.ToString(),
                    Price = t.Price,
                    AttendeeName = t.Booking.User.FullName,
                    AttendeeEmail = t.Booking.User.Email
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound();

            return View("PrintTicket", ticket);
        }

    }
}
