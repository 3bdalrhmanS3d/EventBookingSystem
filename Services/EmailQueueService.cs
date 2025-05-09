using MailKit.Security;
using MimeKit;
using System.Collections.Concurrent;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace EventBookingSystemV1.Services
{
    /// <summary>
    /// A background queue-based email sender using MailKit.
    /// </summary>
    public class EmailQueueService
    {
        private readonly ConcurrentQueue<(string Email, string FullName, string Code, string ResetLink, bool IsResend)> _emailQueue
            = new();
        private readonly IConfiguration _config;

        public EmailQueueService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Enqueue a verification email (signup or resend).
        /// </summary>
        public void QueueVerificationEmail(string email, string fullName, string code, bool isResend = false)
            => _emailQueue.Enqueue((email, fullName, code, null, isResend));

        /// <summary>
        /// Enqueue a password reset email.
        /// </summary>
        public void QueueResetPasswordEmail(string email, string fullName, string resetLink)
            => _emailQueue.Enqueue((email, fullName, null, resetLink, false));

        /// <summary>
        /// Process all queued emails asynchronously.
        /// </summary>
        public async Task ProcessQueueAsync()
        {
            while (_emailQueue.TryDequeue(out var item))
            {
                await SendEmailAsync(item.Email, item.FullName, item.Code, item.ResetLink, item.IsResend);
            }
        }

        private async Task SendEmailAsync(string emailAddress, string fullName, string code, string resetLink, bool isResend)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = Convert.ToInt32((_config["EmailSettings:Port"]));
            var fromEmail = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EventBooking", fromEmail));
            message.To.Add(new MailboxAddress(fullName, emailAddress));

            if (!string.IsNullOrEmpty(resetLink))
            {
                message.Subject = "🔐 Reset Your Password";
                message.Body = new TextPart("html") { Text = BuildResetPasswordEmail(fullName, resetLink) };
            }
            else
            {
                message.Subject = isResend
                    ? "🔄 Resend: Email Verification Code"
                    : "✅ Your Email Verification Code";
                message.Body = new TextPart("html")
                {
                    Text = BuildVerificationEmail(fullName, code, isResend)
                };
            }

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(fromEmail, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string BuildVerificationEmail(string fullName, string code, bool isResend)
        {
            string title = isResend ? "🔄 Resend: Your New Verification Code" : "✅ Your Email Verification Code";
            string message = isResend
                ? "You have requested a new verification code. Please use the code below:"
                : "Please use the following verification code to complete your registration:";

            return $@"
                    <html>
                    <head>
                      <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; text-align: center; }}
                        .container {{ max-width: 500px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                        h2 {{ color: #2d89ef; }}
                        .code {{ font-size:24px; font-weight:bold; color:#d9534f; background:#f8d7da; padding:10px 20px; display:inline-block; border-radius:5px; }}
                        .footer {{ margin-top:20px; font-size:12px; color:#777; }}
                      </style>
                    </head>
                    <body>
                      <div class='container'>
                        <h2>{title}</h2>
                        <p>Hello, {fullName}</p>
                        <p>{message}</p>
                        <div class='code'>{code}</div>
                        <p>If you did not request this email, please ignore it.</p>
                        <div class='footer'>
                          <p>EventBooking System Team</p>
                          <p>Contact: <a href='mailto:support@eventbooking.com'>support@eventbooking.com</a></p>
                        </div>
                      </div>
                    </body>
                    </html>";
        }

        private string BuildResetPasswordEmail(string fullName, string resetLink)
        {
            return $@"
                <html>
                <head>
                    <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; text-align: center; }}
                    .container {{ max-width: 500px; margin: 20px auto; padding: 20px; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                    h2 {{ color: #2d89ef; }}
                    .btn {{ display:inline-block; padding:10px 20px; background:#28a745; color:white; text-decoration:none; border-radius:5px; font-weight:bold; }}
                    .footer {{ margin-top:20px; font-size:12px; color:#777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                    <h2>🔐 Reset Your Password</h2>
                    <p>Hello, {fullName}</p>
                    <p>You requested to reset your password. Click below:</p>
                    <a href='{resetLink}' class='btn'>Reset Password</a>
                    <p>If you did not request this email, ignore this message.</p>
                    <div class='footer'>
                        <p>EventBooking System Team</p>
                    </div>
                    </div>
                </body>
                </html>";
        }
    }
}
