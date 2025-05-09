using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class SignInDto
    {
        [Required, EmailAddress]
        public required string EmailAddress { get; set; }
        [Required, DataType(DataType.Password)]
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
