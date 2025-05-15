using MailKit.Security;
using MimeKit;
using System.Collections.Concurrent;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using EventBookingSystemV1.Models;

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
        public async Task SendBookingTicketEmail(string emailAddress, string fullName, Booking booking, Event eventEntity, decimal totalAmount)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = Convert.ToInt32(_config["EmailSettings:Port"]);
            var fromEmail = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EventBooking", fromEmail));
            message.To.Add(new MailboxAddress(fullName, emailAddress));

            message.Subject = "Your Event Booking Confirmation - EventBooking";
            message.Body = new TextPart("html")
            {
                Text = BuildBookingConfirmationEmail(fullName, booking, eventEntity, totalAmount)
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(fromEmail, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string BuildBookingConfirmationEmail(string fullName, Booking booking, Event eventEntity, decimal totalAmount)
        {
            return $@"
            <html>
            <body>
                <h2>Booking Confirmation</h2>
                <p>Dear {fullName},</p>
                <p>Your booking for the event <strong>{eventEntity.Title}</strong> is confirmed.</p>
                <p><strong>Booking Reference:</strong> {booking.BookingReference}</p>
                <p><strong>Event Date:</strong> {eventEntity.Date.ToString("MMMM dd, yyyy")}</p>
                <p><strong>Venue:</strong> {eventEntity.Venue?.Name ?? "TBD"}</p>
                <p><strong>Total Amount:</strong> ${totalAmount}</p>
                <p>Thank you for booking with EventBooking!</p>
            </body>
            </html>";
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
                message.Subject = "Reset Your Password - EventBooking";
                message.Body = new TextPart("html") { Text = BuildResetPasswordEmail(fullName, resetLink) };
            }
            else
            {
                message.Subject = isResend
                    ? "Your New Verification Code - EventBooking"
                    : "Verify Your Email - EventBooking";
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
            string title = isResend ? "Your New Verification Code" : "Verify Your Email";
            string message = isResend
                ? "You have requested a new verification code. Please use the code below to verify your account:"
                : "Thank you for registering with EventBooking. Please use the following verification code to complete your registration:";

            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{title}</title>
                    <style>
                        :root {{
                            --primary-color: #10b981;
                            --primary-hover: #059669;
                            --primary-light: #d1fae5;
                            --secondary-color: #0f766e;
                            --text-color: #1e293b;
                            --text-light: #64748b;
                            --background-color: #ffffff;
                            --background-light: #f8fafc;
                            --border-color: #e2e8f0;
                        }}
        
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f8fafc;
                            color: #1e293b;
                            line-height: 1.6;
                        }}
        
                        .email-container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            border-radius: 8px;
                            overflow: hidden;
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
                        }}
        
                        .email-header {{
                            background: linear-gradient(to right, #10b981, #0f766e);
                            padding: 30px 20px;
                            text-align: center;
                        }}
        
                        .email-header h1 {{
                            color: white;
                            margin: 0;
                            font-size: 24px;
                            font-weight: 600;
                        }}
        
                        .email-body {{
                            padding: 30px;
                        }}
        
                        .greeting {{
                            font-size: 18px;
                            margin-bottom: 20px;
                        }}
        
                        .message {{
                            margin-bottom: 30px;
                        }}
        
                        .verification-code {{
                            background-color: #d1fae5;
                            color: #10b981;
                            font-size: 32px;
                            font-weight: bold;
                            letter-spacing: 5px;
                            text-align: center;
                            padding: 20px;
                            border-radius: 8px;
                            margin: 30px 0;
                        }}
        
                        .note {{
                            font-size: 14px;
                            color: #64748b;
                            margin-top: 30px;
                            padding-top: 20px;
                            border-top: 1px solid #e2e8f0;
                        }}
        
                        .email-footer {{
                            background-color: #f8fafc;
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #64748b;
                        }}
        
                        .logo {{
                            font-size: 22px;
                            font-weight: bold;
                            color: white;
                            text-decoration: none;
                        }}
        
                        .social-links {{
                            margin-top: 15px;
                        }}
        
                        .social-links a {{
                            display: inline-block;
                            margin: 0 10px;
                            color: #10b981;
                            text-decoration: none;
                        }}
        
                        @media only screen and (max-width: 600px) {{
                            .email-container {{
                                width: 100%;
                                border-radius: 0;
                            }}
            
                            .email-header {{
                                padding: 20px 15px;
                            }}
            
                            .email-body {{
                                padding: 20px 15px;
                            }}
            
                            .verification-code {{
                                font-size: 28px;
                                letter-spacing: 3px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <span class='logo'>EventBooking</span>
                            <h1>{title}</h1>
                        </div>
        
                        <div class='email-body'>
                            <div class='greeting'>Hello, {fullName}!</div>
            
                            <div class='message'>
                                {message}
                            </div>
            
                            <div class='verification-code'>{code}</div>
            
                            <div>
                                This code will expire in 6 hours. If you did not request this verification code, please ignore this email.
                            </div>
            
                            <div class='note'>
                                For security reasons, please do not share this code with anyone.
                                If you're having trouble, please contact our support team.
                            </div>
                        </div>
        
                        <div class='email-footer'>
                            <div>&copy; {DateTime.Now.Year} EventBooking. All rights reserved.</div>
                            <div>123 Event Street, Cairo, Egypt</div>
                            <div class='social-links'>
                                <a href='#'>Facebook</a>
                                <a href='#'>Twitter</a>
                                <a href='#'>Instagram</a>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string BuildResetPasswordEmail(string fullName, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Reset Your Password</title>
                    <style>
                        :root {{
                            --primary-color: #10b981;
                            --primary-hover: #059669;
                            --primary-light: #d1fae5;
                            --secondary-color: #0f766e;
                            --text-color: #1e293b;
                            --text-light: #64748b;
                            --background-color: #ffffff;
                            --background-light: #f8fafc;
                            --border-color: #e2e8f0;
                        }}
        
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #f8fafc;
                            color: #1e293b;
                            line-height: 1.6;
                        }}
        
                        .email-container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            border-radius: 8px;
                            overflow: hidden;
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
                        }}
        
                        .email-header {{
                            background: linear-gradient(to right, #10b981, #0f766e);
                            padding: 30px 20px;
                            text-align: center;
                        }}
        
                        .email-header h1 {{
                            color: white;
                            margin: 0;
                            font-size: 24px;
                            font-weight: 600;
                        }}
        
                        .email-body {{
                            padding: 30px;
                        }}
        
                        .greeting {{
                            font-size: 18px;
                            margin-bottom: 20px;
                        }}
        
                        .message {{
                            margin-bottom: 30px;
                        }}
        
                        .button-container {{
                            text-align: center;
                            margin: 30px 0;
                        }}
        
                        .button {{
                            display: inline-block;
                            background-color: #10b981;
                            color: white;
                            text-decoration: none;
                            padding: 12px 30px;
                            border-radius: 4px;
                            font-weight: 600;
                            font-size: 16px;
                        }}
        
                        .button:hover {{
                            background-color: #059669;
                        }}
        
                        .expiry-note {{
                            font-size: 14px;
                            margin-top: 20px;
                            text-align: center;
                        }}
        
                        .note {{
                            font-size: 14px;
                            color: #64748b;
                            margin-top: 30px;
                            padding-top: 20px;
                            border-top: 1px solid #e2e8f0;
                        }}
        
                        .email-footer {{
                            background-color: #f8fafc;
                            padding: 20px;
                            text-align: center;
                            font-size: 12px;
                            color: #64748b;
                        }}
        
                        .logo {{
                            font-size: 22px;
                            font-weight: bold;
                            color: white;
                            text-decoration: none;
                        }}
        
                        .social-links {{
                            margin-top: 15px;
                        }}
        
                        .social-links a {{
                            display: inline-block;
                            margin: 0 10px;
                            color: #10b981;
                            text-decoration: none;
                        }}
        
                        .link-fallback {{
                            margin-top: 15px;
                            word-break: break-all;
                            font-size: 12px;
                            color: #64748b;
                        }}
        
                        @media only screen and (max-width: 600px) {{
                            .email-container {{
                                width: 100%;
                                border-radius: 0;
                            }}
            
                            .email-header {{
                                padding: 20px 15px;
                            }}
            
                            .email-body {{
                                padding: 20px 15px;
                            }}
            
                            .button {{
                                display: block;
                                text-align: center;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-container'>
                        <div class='email-header'>
                            <span class='logo'>EventBooking</span>
                            <h1>Reset Your Password</h1>
                        </div>
        
                        <div class='email-body'>
                            <div class='greeting'>Hello, {fullName}!</div>
            
                            <div class='message'>
                                We received a request to reset your password for your EventBooking account. 
                                Click the button below to reset your password:
                            </div>
            
                            <div class='button-container'>
                                <a href='{resetLink}' class='button'>Reset Password</a>
                            </div>
            
                            <div class='expiry-note'>
                                This link will expire in 6 hours.
                            </div>
            
                            <div class='link-fallback'>
                                If the button above doesn't work, copy and paste this link into your browser:
                                <br>
                                {resetLink}
                            </div>
            
                            <div class='note'>
                                If you did not request a password reset, please ignore this email or contact our support team if you have concerns about your account security.
                            </div>
                        </div>
        
                        <div class='email-footer'>
                            <div>&copy; {DateTime.Now.Year} EventBooking. All rights reserved.</div>
                            <div>123 Event Street, Cairo, Egypt</div>
                            <div class='social-links'>
                                <a href='#'>Facebook</a>
                                <a href='#'>Twitter</a>
                                <a href='#'>Instagram</a>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}