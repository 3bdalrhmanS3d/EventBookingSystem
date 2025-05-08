using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    // Tag entity for flexible event tagging
    public class Tag : ISoftDelete
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<EventTag> EventTags { get; set; }
    }
}
