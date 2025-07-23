using Microsoft.Extensions.Options;
using SistemaBodega.Helpers;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SistemaBodega.Services
{
    public class EmailService
    {
        private readonly EmailProvidersSettings _emailSettings;

        public EmailService(IOptions<EmailProvidersSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, string providerName = null)
        {
            providerName ??= _emailSettings.DefaultProvider;

            if (!_emailSettings.Providers.ContainsKey(providerName))
                throw new ArgumentException($"Proveedor SMTP '{providerName}' no está configurado.");

            var smtpConfig = _emailSettings.Providers[providerName];

            using var client = new SmtpClient(smtpConfig.Host, smtpConfig.Port)
            {
                EnableSsl = smtpConfig.EnableSsl,
                Credentials = new NetworkCredential(smtpConfig.User, smtpConfig.Password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(smtpConfig.User),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
