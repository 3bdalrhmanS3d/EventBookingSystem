using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.Models
{
    // Audit log for tracking entity changes
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public required string EntityName { get; set; }
        public required int EntityId { get; set; }
        public required string Action { get; set; } // Create, Update, Delete

        public required string PerformedBy { get; set; }
        public DateTimeOffset PerformedAt { get; set; } = DateTimeOffset.UtcNow;

        public required string ChangesJson { get; set; }
    }
}
