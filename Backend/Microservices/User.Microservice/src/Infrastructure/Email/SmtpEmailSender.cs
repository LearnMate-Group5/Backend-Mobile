using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using SharedLibrary.Configs;

namespace Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EnvironmentConfig _config;

    public SmtpEmailSender(EnvironmentConfig config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_config.SmtpHost) ||
            string.IsNullOrWhiteSpace(_config.SmtpUsername) ||
            string.IsNullOrWhiteSpace(_config.SmtpPassword))
        {
            throw new InvalidOperationException("SMTP configuration is missing. Please configure SMTP_HOST, SMTP_USER/SMTP_USERNAME, and SMTP_PASS/SMTP_PASSWORD environment variables.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_config.SmtpFromAddress, _config.SmtpFromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        using var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort)
        {
            EnableSsl = _config.SmtpEnableSsl,
            Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword)
        };

        using var registration = cancellationToken.Register(client.SendAsyncCancel);
        await client.SendMailAsync(message);
    }
}
