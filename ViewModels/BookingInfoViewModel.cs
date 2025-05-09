namespace EventBookingSystemV1.ViewModels
{
    public class BookingInfoViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public DateTimeOffset BookedAt { get; set; }
        public int TicketCount { get; set; }
    }
}