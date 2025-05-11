using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class VenueDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }

        [Required]
        [StringLength(100)]
        public string ContactInfo { get; set; } = string.Empty;
    }
}
