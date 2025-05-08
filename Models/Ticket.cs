using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    public class Ticket : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        public required int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
        public string TicketType { get; set; }
        public decimal Price { get; set; }
        public bool IsDeleted { get; set; }

        public DateTimeOffset CreatedAt = DateTimeOffset.UtcNow;
    }
}
