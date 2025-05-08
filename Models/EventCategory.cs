using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    public class EventCategory : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }

        // Navigation: events under this category
        public virtual ICollection<Event> Events { get; set; }

        public bool IsDeleted { get; set; }
    }
}
