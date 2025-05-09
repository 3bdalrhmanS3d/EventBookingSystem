namespace EventBookingSystemV1.ViewModels
{
    public class ReviewInfoViewModel
    {
        public string Reviewer { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}