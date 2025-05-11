using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EventBookingSystemV1.Models
{
    // User roles in the system
    public enum UserRole
    {
        User,
        Admin
    }
    public class User : ISoftDelete
    {
        [Key]
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string PasswordHash { get; set; }
        public required UserRole Role { get; set; } = UserRole.User;
        public string ProfilePicture { get; set; } 
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public required DateTimeOffset BirthDate { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
        public virtual ICollection<AccountActivation> AccountActivation { get; set; }

        public User()
        {
            Bookings = new List<Booking>();
            Reviews = new List<Review>();
            Favorites = new List<Favorite>();
            Notifications = new List<Notification>();
            PasswordResetTokens = new List<PasswordResetToken>();
            AccountActivation = new List<AccountActivation>();
        }
    }
}
