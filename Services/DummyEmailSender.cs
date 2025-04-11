using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System; // For Console.WriteLine

namespace SmartInventory.Services
{
    // Dummy implementation for IEmailSender for development/testing purposes.
    // In a production environment, replace this with a real email service (SendGrid, Mailgun, SMTP, etc.)
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine("--- Dummy Email Sender ---");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body (HTML): {htmlMessage}");
            Console.WriteLine("--- End Dummy Email ---");

            // Return a completed task since this is synchronous for the dummy implementation
            return Task.CompletedTask;
        }
    }
}
