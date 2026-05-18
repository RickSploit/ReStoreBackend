using ReStore.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ReStore.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
                var fromEmail = smtpSettings["FromEmail"] ?? username;

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(username, password),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, "ReStore"),
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = htmlMessage
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                // Log error in production
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendConfirmationEmailAsync(string toEmail, string userId, string token)
        {
            var baseUrl = _configuration["EmailSettings:ConfirmationBaseUrl"] ?? "http://localhost:5000";
            var confirmationLink = $"{baseUrl}/api/auth/confirm-email?userId={userId}&token={WebUtility.UrlEncode(token)}";

            var htmlMessage = @"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #333;'>Welcome to ReStore!</h2>
                    <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                    <div style='margin: 30px 0;'>
                        <a href='" + confirmationLink + @"' 
                           style='background-color: #4CAF50; color: white; padding: 15px 30px; 
                                  text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            Confirm Email
                        </a>
                    </div>
                    <p>Or copy and paste this link:</p>
                    <p style='color: #666; word-break: break-all;'>" + confirmationLink + @"</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='color: #999; font-size: 12px;'>If you didn't create an account, please ignore this email.</p>
                </body>
                </html>";

            return await SendEmailAsync(toEmail, "Confirm your ReStore Account", htmlMessage);
        }
    }
}
