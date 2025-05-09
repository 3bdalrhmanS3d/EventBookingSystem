namespace EventBookingSystemV1.Services
{
    // Defines how to send e-mails from the application.
    // This interface comes from
    // Microsoft.AspNetCore.Identity.UI.Services
    // but you can also declare your own if you prefer.
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email with the specified subject and body to the given address.
        /// </summary>
        /// <param name="email">The recipient address.</param>
        /// <param name="subject">The e-mail subject line.</param>
        /// <param name="htmlMessage">The e-mail body (HTML or plain text).</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
