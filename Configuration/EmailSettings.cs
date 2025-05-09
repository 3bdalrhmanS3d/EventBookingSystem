namespace EventBookingSystemV1.Configuration
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }   // maps to "SmtpServer"
        public int Port { get; set; }   // maps to "Port"
        public string Email { get; set; }   // maps to "Email"
        public string Password { get; set; }   // maps to "Password"
    }
}
