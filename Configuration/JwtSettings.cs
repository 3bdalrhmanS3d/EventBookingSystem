namespace EventBookingSystemV1.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = default!;
        public string ValidIss { get; set; } = default!;
        public string ValidAud { get; set; } = default!;
        public int DurationInMinutes { get; set; }
    }

}
