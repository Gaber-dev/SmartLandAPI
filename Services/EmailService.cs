using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;


namespace SmartLandAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Email address is required.", nameof(to));

            try
            {
                var toAddress = new MailAddress(to); 
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid email address format.", nameof(to));
            }

            // Fetch values from appsettings.json
            var smtpSection = _configuration.GetSection("Jwt:Smtp");
            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"]);
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, "SmartLand"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
