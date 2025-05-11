using EventBookingSystemV1.Models;

namespace EventBookingSystemV1.ViewModels
{
    public class TicketViewModel
    {
        public string TicketType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}