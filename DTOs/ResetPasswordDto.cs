using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        [RegularExpression(
            "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*()_+\\-={}\\[\\]\\|;:'\\\",.<>/?]).+$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
        [Display(Name = "NewPassword")]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password), Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
