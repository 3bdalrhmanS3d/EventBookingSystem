namespace EventBookingSystemV1.ViewModels
{
    public class EventDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Venue { get; set; } = default!;
        public DateTimeOffset Date { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = default!;

        public string OrganizerEmail { get; set; }
        public string OrganizerPhone { get; set; }
        // Related bookings
        public List<BookingInfoViewModel> Bookings { get; set; } = new();

        // Related reviews
        public List<ReviewInfoViewModel> Reviews { get; set; } = new();

        public bool IsFavorited { get; set; }

        public double AverageRating { get; set; }

    }
}