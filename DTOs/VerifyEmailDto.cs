using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class VerifyEmailDto
    {
        [EmailAddress]
        public required string EmailAddress { get; set; }

        [Required(ErrorMessage = "Activation code is required.")]
        public string Token { get; set; }

    }
}
