using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    public class Venue : ISoftDelete
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public int Capacity { get; set; }        
        public required string ContactInfo { get; set; }

        public bool IsDeleted { get; set; }
        // Navigation: events at this venue
        public virtual ICollection<Event> Events { get; set; }

        public Venue() { 
            Events = new List<Event>();
        }

    }
}
