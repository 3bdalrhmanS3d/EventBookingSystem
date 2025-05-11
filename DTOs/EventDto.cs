using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace EventBookingSystemV1.DTOs
{
    public class EventDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int EventCategoryId { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        public DateTimeOffset Date { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public IFormFile Image { get; set; }  // إضافة صورة الحدث
    }

}
