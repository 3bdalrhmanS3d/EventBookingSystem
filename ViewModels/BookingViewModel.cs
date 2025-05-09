namespace EventBookingSystemV1.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public string EventTitle { get; set; } = default!;
        public DateTimeOffset EventDate { get; set; }
        public DateTimeOffset BookedAt { get; set; }
    }
}