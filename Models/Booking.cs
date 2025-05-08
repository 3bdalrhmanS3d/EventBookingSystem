using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace EventBookingSystemV1.Models
{
    public class Booking : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        public required int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public required int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public bool IsDeleted { get; set; }
        public required DateTimeOffset BookedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
