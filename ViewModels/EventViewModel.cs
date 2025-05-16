namespace EventBookingSystemV1.ViewModels
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public string VenueName { get; set; } = default!;
        public DateTimeOffset Date { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = default!;
        public string OrganizerEmail { get; set; }
        public string OrganizerPhone { get; set; }

        public double AvgRate { get; set; }
    }
}
