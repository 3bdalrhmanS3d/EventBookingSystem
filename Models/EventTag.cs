using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    // Junction table for Event-Tag many-to-many
    public class EventTag : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        public required int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public required int TagId { get; set; }
        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

        public bool IsDeleted { get; set; }
    }
}
