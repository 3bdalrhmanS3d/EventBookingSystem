using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    /// Stores the activation code sent to a user for verifying their account.

    public class AccountActivation
    {
        [Key]
        public int Id { get; set; }
        public required int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public required string Code { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTimeOffset? UsedAt { get; set; }
    }
}
