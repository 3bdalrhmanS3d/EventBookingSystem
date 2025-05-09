using EventBookingSystemV1.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace EventBookingSystemV1.Services
{
    public class EmailSender
    {
        private readonly EmailSettings _opts;
        public EmailSender(IOptions<EmailSettings> opts) => _opts = opts.Value;
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_opts.SmtpServer, _opts.Port)
            {
                Credentials = new NetworkCredential(_opts.Email, _opts.Password),
                EnableSsl = true
            };

            var msg = new MailMessage(_opts.Email, to, subject, body) { IsBodyHtml = false };
            await client.SendMailAsync(msg);
        }
    }
}
