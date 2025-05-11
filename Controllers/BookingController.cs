using EventBookingSystemV1.Data;
using EventBookingSystemV1.DTOs;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystemV1.Controllers
{
    [Route("booking")]
    [Authorize]
    public class BookingController : BaseController
    {
        public BookingController(ApplicationDbContext context, IWebHostEnvironment env)
            : base(context, env)
        {
        }

        // GET /booking/create/5
        [HttpGet("create/{eventId}")]
        public async Task<IActionResult> Create(int eventId)
        {
            var userId = CurrentUserId;

            // 1) prevent double‐booking
            if (await _context.Bookings.AnyAsync(b => b.UserId == userId && b.EventId == eventId))
            {
                TempData["ErrorMessage"] = "You have already booked this event.";
                return RedirectToAction("MyBookings", "Profile", new { id = eventId });
            }

            // 2) load event & user
            var @event = await _context.Events
                .AsNoTracking()
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("Index", "Event");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("SignIn", "Account");

            // 3) assemble ticket types
            var ticketTypes = new List<TicketTypeViewModel>
            {
                new TicketTypeViewModel {
                    Id          = "Regular",
                    Name        = "Regular Ticket",
                    Description = "Standard admission to the event",
                    Price       = @event.Price,
                    IsAvailable = true
                },
                new TicketTypeViewModel {
                    Id          = "VIP",
                    Name        = "VIP Ticket",
                    Description = "Premium seating and exclusive perks",
                    Price       = @event.Price * 1.5m,
                    IsAvailable = true
                },
                new TicketTypeViewModel {
                    Id          = "Early",
                    Name        = "Early Bird",
                    Description = "Discounted rate for early bookings",
                    Price       = @event.Price * 0.8m,
                    IsAvailable = DateTime.UtcNow < @event.Date.AddDays(-14)
                },
            };

            var vm = new BookingCreateDto
            {
                EventId = eventId,
                
                Name = user.FullName,
                Email = user.Email,
                AvailableTicketTypes = ticketTypes,
                TicketQuantities = new Dictionary<string, int>()
            };

            return View(vm);
        }

        // POST /booking/create/5
        [HttpPost("create/{eventId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId, BookingCreateDto model)
        {
            
            var userId = CurrentUserId;

            // ensure at least one ticket chosen
            if (model.TicketQuantities == null || !model.TicketQuantities.Any(kvp => kvp.Value > 0))
            {
                ModelState.AddModelError("", "Please select at least one ticket.");

                // Rebuild the view model for redisplay
                var @event = await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (@event == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("Index", "Events");
                }

                // Reassemble ticket types
                var ticketTypes = new List<TicketTypeViewModel>
                {
                    new TicketTypeViewModel {
                        Id          = "Regular",
                        Name        = "Regular Ticket",
                        Description = "Standard admission to the event",
                        Price       = @event.Price,
                        IsAvailable = true
                    },
                    new TicketTypeViewModel {
                        Id          = "VIP",
                        Name        = "VIP Ticket",
                        Description = "Premium seating and exclusive perks",
                        Price       = @event.Price * 1.5m,
                        IsAvailable = true
                    },
                    new TicketTypeViewModel {
                        Id          = "Early",
                        Name        = "Early Bird",
                        Description = "Discounted rate for early bookings",
                        Price       = @event.Price * 0.8m,
                        IsAvailable = DateTime.UtcNow < @event.Date.AddDays(-5)
                    },
                };

                model.AvailableTicketTypes = ticketTypes;                

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                // Rebuild the view model for redisplay
                var @event = await _context.Events
                    .AsNoTracking()
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (@event == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("Index", "Event");
                }

                // Reassemble ticket types
                var ticketTypes = new List<TicketTypeViewModel>
                {
                    new TicketTypeViewModel {
                        Id          = "Regular",
                        Name        = "Regular Ticket",
                        Description = "Standard admission to the event",
                        Price       = @event.Price,
                        IsAvailable = true
                    },
                    new TicketTypeViewModel {
                        Id          = "VIP",
                        Name        = "VIP Ticket",
                        Description = "Premium seating and exclusive perks",
                        Price       = @event.Price * 1.5m,
                        IsAvailable = true
                    },
                    new TicketTypeViewModel {
                        Id          = "Early",
                        Name        = "Early Bird",
                        Description = "Discounted rate for early bookings",
                        Price       = @event.Price * 0.8m,
                        IsAvailable = DateTime.UtcNow < @event.Date.AddDays(-14)
                    },
                };

                model.AvailableTicketTypes = ticketTypes;
                
               
                return View(model);
            }

            // prevent double‐booking
            if (await _context.Bookings.AnyAsync(b => b.UserId == userId && b.EventId == eventId))
            {
                TempData["ErrorMessage"] = "You have already booked this event.";
                return RedirectToAction("MyBookings", "Profile", new { id = eventId });
            }

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("Index", "Event");
            }

            try
            {
                // 1) make booking
                var booking = new Booking
                {
                    EventId = eventId,
                    UserId = userId,
                    BookedAt = DateTimeOffset.UtcNow,
                    BookingReference = GenerateBookingReference()
                };
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // 2) create tickets
                decimal total = 0;
                foreach (var kvp in model.TicketQuantities)
                {
                    var typeKey = kvp.Key;
                    var qty = kvp.Value;
                    if (qty <= 0) continue;

                    decimal price = typeKey.ToLower() switch
                    {
                        "vip" => eventEntity.Price * 1.5m,
                        "early" => eventEntity.Price * 0.8m,
                        _ => eventEntity.Price
                    };

                    total += price * qty;
                    for (int i = 0; i < qty; i++)
                    {
                        _context.Tickets.Add(new Ticket
                        {
                            BookingId = booking.Id,
                            TicketType = Enum.Parse<TicketT>(typeKey, ignoreCase: true),
                            Price = price,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();

                // 3) notify & audit
                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Message = $"Your booking for '{eventEntity.Title}' is confirmed.",
                    CreatedAt = DateTimeOffset.UtcNow
                });
                await _context.SaveChangesAsync();

                await LogAuditAsync("Booking", booking.Id, "Create", new { booking.EventId, booking.UserId, Total = total });

                // 4) go to confirmation
                return RedirectToAction(nameof(Confirmation), new { id = booking.Id });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating booking: {ex.Message}");

                // Add error message
                TempData["ErrorMessage"] = "An error occurred while processing your booking. Please try again.";

                // Redirect back to the event details
                return RedirectToAction("Details", "Event", new { id = eventId });
            }
        }

        // GET /booking/confirmation/{id}
        [HttpGet("confirmation/{id}")]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = CurrentUserId;
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Event).ThenInclude(e => e.Venue)
                .Include(b => b.Tickets)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null) return NotFound();

            var ticketGroups = booking.Tickets
                .GroupBy(t => t.TicketType)
                .Select(g => new TicketViewModel
                {
                    TicketType = g.Key.ToString(),
                    Quantity = g.Count(),
                    Price = g.First().Price
                })
                .ToList();

            var vm = new BookingConfirmationViewModel
            {
                BookingId = booking.Id,
                BookingReference = booking.BookingReference!,
                EventId = booking.EventId,
                EventTitle = booking.Event.Title,
                EventDate = booking.Event.Date,
                EventVenue = booking.Event.Venue?.Name ?? "TBD",
                EventImageUrl = booking.Event.ImageUrl ?? "/images/event-placeholder.jpg",
                AttendeeName = booking.User.FullName,
                AttendeeEmail = booking.User.Email,
                Tickets = ticketGroups,
                TotalAmount = ticketGroups.Sum(t => t.Price * t.Quantity)
            };

            return View(vm);
        }

        private string GenerateBookingReference()
        {
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            string rand(int len) => new string(Enumerable
                .Repeat(chars, len)
                .Select(s => s[rnd.Next(s.Length)])
                .ToArray());
            return $"BK-{rand(4)}-{rand(4)}";
        }
    }
}