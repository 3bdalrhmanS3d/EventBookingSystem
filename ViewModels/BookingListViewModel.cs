namespace EventBookingSystemV1.ViewModels
{
    internal class BookingListViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string EventTitle { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public DateTimeOffset BookedAt { get; set; }
        public int TicketCount { get; set; }
    }
}