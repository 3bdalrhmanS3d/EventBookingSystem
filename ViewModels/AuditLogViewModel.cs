namespace EventBookingSystemV1.ViewModels
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public DateTimeOffset PerformedAt { get; set; }
        public string ChangesJson { get; set; }
    }
}