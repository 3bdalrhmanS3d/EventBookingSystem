namespace EventBookingSystemV1.Configuration
{
    public class JwtSettings
    {
        public string ValidIss { get; set; }   // maps to "ValidIss"
        public string ValidAud { get; set; }   // maps to "ValidAud"
        public string SecretKey { get; set; }   // maps to "SecretKey"
        public int DurationInMinutes { get; set; } = 60; // default if you like
    }
}
