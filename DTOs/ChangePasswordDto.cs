using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    public class ChangePasswordDto
    {
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

        [Required(ErrorMessage = "Old password is required.")]
        public string? OldPassword { get; set; } // Optional, for changing password
    }
}