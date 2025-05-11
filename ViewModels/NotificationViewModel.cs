namespace EventBookingSystemV1.ViewModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = default!;
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}