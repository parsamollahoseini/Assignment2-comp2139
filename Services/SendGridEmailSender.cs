using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Add logger

namespace SmartInventory.Services
{
    // Helper class to hold SendGrid options (could be defined elsewhere)
    public class SendGridOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = "noreply@yourdomain.com"; // TODO: Configure a real sender email
        public string SenderName { get; set; } = "Smart Inventory System"; // TODO: Configure sender name
    }

    public class SendGridEmailSender : IEmailSender
    {
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly SendGridOptions _options;

        public SendGridEmailSender(IOptions<SendGridOptions> optionsAccessor, ILogger<SendGridEmailSender> logger)
        {
            _options = optionsAccessor.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                _logger.LogError("SendGrid ApiKey is not configured. Email not sent to {Email}", email);
                // Consider throwing an exception or handling this more gracefully
                // For now, just log and return to avoid breaking the flow if key is missing
                return;
                // throw new System.Exception("SendGrid ApiKey is not configured.");
            }

            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.SenderEmail, _options.SenderName);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage); // Use htmlMessage for HTML content

            try
            {
                var response = await client.SendEmailAsync(msg);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email to {Email} queued successfully! Subject: {Subject}", email, subject);
                }
                else
                {
                    _logger.LogError("Failed to send email to {Email}. Status Code: {StatusCode}, Body: {Body}",
                        email, response.StatusCode, await response.Body.ReadAsStringAsync());
                    // Consider how to handle failures - throw exception, retry, etc.
                }
            }
            catch (System.Exception ex)
            {
                 _logger.LogError(ex, "Exception occurred while sending email to {Email}", email);
                 // Rethrow or handle as appropriate for your application's error handling strategy
                 throw;
            }
        }
    }
}
