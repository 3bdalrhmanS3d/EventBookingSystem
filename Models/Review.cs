using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    public class Review : ISoftDelete
    {
        [Key]
        public int Id { get; set; }
        public required int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public required int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        public int Rating { get; set; }
        public bool IsDeleted { get; set; }

        public required string Comment { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
