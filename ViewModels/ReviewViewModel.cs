namespace EventBookingSystemV1.ViewModels
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string EventTitle { get; set; } = default!;
        public int EventId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}