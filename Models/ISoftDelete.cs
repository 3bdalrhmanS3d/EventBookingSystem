namespace EventBookingSystemV1.Models
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
