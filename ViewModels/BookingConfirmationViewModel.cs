using System.Collections.Generic;

namespace EventBookingSystemV1.ViewModels
{
    public class BookingConfirmationViewModel
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string EventVenue { get; set; }
        public string EventImageUrl { get; set; }
        public string AttendeeName { get; set; }
        public string AttendeeEmail { get; set; }
        public List<TicketViewModel> Tickets { get; set; }
        public decimal TotalAmount { get; set; }
    }
}