using EventBookingSystemV1.Models;

namespace EventBookingSystemV1.ViewModels
{
    public class MyTicketViewModel
    {
        public int Id { get; set; }
        public string EventTitle { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public string BookingReference { get; set; }
        public TicketT TicketType { get; set; }
    }
}