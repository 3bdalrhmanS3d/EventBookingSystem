using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public required string EmailAddress { get; set; }
    }
}
