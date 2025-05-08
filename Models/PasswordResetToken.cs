using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        public  required int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public required string Token { get; set; }

        public DateTimeOffset ExpiryDate { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
