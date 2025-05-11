using EventBookingSystemV1.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class BookingCreateDto
    {
        // Event info
        public int EventId { get; set; }
        
        
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        public Dictionary<string, int> TicketQuantities { get; set; } = new Dictionary<string, int>();

        [BindNever]               // or [ValidateNever]
        public List<TicketTypeViewModel> AvailableTicketTypes { get; set; } = new();

    }
}
