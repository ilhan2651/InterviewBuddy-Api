using Buddy.Application.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Buddy.Infrastructure.Services.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailVerificationAsync(
            string toEmail,
            string fullName,
            string verificationLink,
            CancellationToken cancellationToken = default)
        {
            var host = _configuration["Smtp:Host"];
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromName = _configuration["Smtp:FromName"] ?? "StudyBuddy";
            var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var subject = "Email adresini dogrula";
            var body = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <h2>Merhaba {fullName},</h2>
                    <p>StudyBuddy hesabini aktif etmek icin email adresini dogrulaman gerekiyor.</p>
                    <p>
                        <a href='{verificationLink}'
                           style='display:inline-block;padding:12px 20px;background:#22c55e;color:#fff;text-decoration:none;border-radius:8px;'>
                           Email adresimi dogrula
                        </a>
                    </p>
                    <p>Buton calismazsa bu linki kullan:</p>
                    <p>{verificationLink}</p>
                    <p>Bu link 24 saat gecerlidir.</p>
                </div>";

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
