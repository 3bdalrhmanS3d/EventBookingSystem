namespace EventBookingSystemV1.ViewModels
{
    public class VenueViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
    }
}
