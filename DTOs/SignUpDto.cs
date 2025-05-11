using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    /// <summary>
    /// Data Transfer Object for user signup.
    /// </summary>
    public class SignUpDto : IValidatableObject
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Last name can only contain letters.")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(254, ErrorMessage = "Email address cannot exceed 254 characters.")]
        [Display(Name = "Email Address")]
        public required string EmailAddress { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        [RegularExpression(
            "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[!@#$%^&*()_+\\-={}\\[\\]\\|;:'\\\",.<>/?]).+$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Birth date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public required DateTimeOffset BirthDate { get; set; }

        [MustBeTrue(ErrorMessage = "You must accept the terms and conditions.")]
        [Display(Name = "I agree to the Terms & Conditions")]
        public bool AcceptTerms { get; set; }


        /// <summary>
        /// Validates that birth date is not in the future and user is at least 13 years old.
        /// </summary>
        /// 
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check birth date not in the future
            if (BirthDate.Date > DateTimeOffset.UtcNow.Date)
            {
                yield return new ValidationResult(
                    "Birth date cannot be in the future.",
                    new[] { nameof(BirthDate) });
            }

            // Age calculation
            var today = DateTimeOffset.UtcNow;
            int age = today.Year - BirthDate.Year;
            if (BirthDate > today.AddYears(-age)) age--;
            if (age < 13)
            {
                yield return new ValidationResult(
                    "You must be at least 13 years old to register.",
                    new[] { nameof(BirthDate) });
            }
        }

    }
}
