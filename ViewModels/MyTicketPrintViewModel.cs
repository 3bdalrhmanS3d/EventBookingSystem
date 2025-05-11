namespace EventBookingSystemV1.ViewModels
{
    internal class MyTicketPrintViewModel
    {
        public int TicketId { get; set; }
        public string EventTitle { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string VenueName { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public string BookingReference { get; set; }
        public string TicketType { get; set; }
        public decimal Price { get; set; }
        public string AttendeeName { get; set; }
        public string AttendeeEmail { get; set; }
    }
}