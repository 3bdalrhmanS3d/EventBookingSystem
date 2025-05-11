using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}