using Services.Helpers.MailServices;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Services.LoggerService.Interface;
using Microsoft.Extensions.Options;

namespace Services.Helpers.MailService
{
    public class EmailService : IEmailService
    {
       
        private readonly EmailConfiguration _emailConfiguration;
        public readonly ILoggerManager _logger;
        public EmailService(ILoggerManager logger, IOptions<EmailConfiguration> emailConfiguration)
        {
            _logger = logger;
            _emailConfiguration = emailConfiguration.Value;
        }
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                _logger.LogInfo($"Attempting to send mail to {toEmail}");

                string smtpHost = _emailConfiguration.Host;
                int smtpPort = _emailConfiguration.Port;
                string userName = _emailConfiguration.Username;
                string smtpPassword = _emailConfiguration.Password;
                bool enableSsl =  _emailConfiguration.EnableSsl;
                using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.EnableSsl = true; 
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(userName, smtpPassword); 

                    // Create the email message
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(userName), 
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = true, 
                    };

                    // Add the recipient's email address
                    mailMessage.To.Add(toEmail);

                    // Send the email
                     smtpClient.Send(mailMessage);

                    return true; // Email sent successfully
                }
            }
            catch(Exception ex) { _logger.LogError($"something happened trying to send email. Details: {ex.StackTrace}"); return false; }
        }
    }
}
