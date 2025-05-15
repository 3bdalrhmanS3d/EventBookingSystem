namespace EventBookingSystemV1.DTOs
{
    public class ProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

    }
}