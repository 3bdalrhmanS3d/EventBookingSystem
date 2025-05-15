using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    public class Event : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        public required string Title { get; set; }

        public required string Description { get; set; }

        public required int EventCategoryId { get; set; }
        [ForeignKey("EventCategoryId")]
        public virtual EventCategory Category { get; set; }

        public required int VenueId { get; set; }
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }

        public required DateTimeOffset Date { get; set; }

        public required decimal Price { get; set; }

        public required string ImageUrl { get; set; }

        public required string OrganizerEmail { get; set; }
        public required string OrganizerPhone { get; set; }
        public bool IsDeleted { get; set; }
        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        public Event()
        {
            Bookings = new List<Booking>();
            Reviews = new List<Review>();
        }
    }
}
