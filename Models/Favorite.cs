using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    public class Favorite : ISoftDelete
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
        public DateTime AddedAt { get; set; } = DateTime  .UtcNow;
    }
}
