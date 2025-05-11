
namespace EventBookingSystemV1.ViewModels
{
    public class BookingCreateViewModel
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Dictionary<string, int> TicketQuantities { get; set; } = new();

    }
}
