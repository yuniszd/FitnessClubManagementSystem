using FCMS.Application.Abstracts;
using FCMS.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace FCMS.Infrastructure.Services;

public class MailKitEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public MailKitEmailService(IConfiguration config)
    {
        _settings = config.GetSection("Smtp").Get<SmtpSettings>()
                    ?? throw new InvalidOperationException("SMTP settings missing");
    }

    public async Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Gym Management", _settings.Username)); // Display Name
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        if (attachment != null)
            builder.Attachments.Add("qrcode.png", attachment, new ContentType("image", "png"));

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Server, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password); // burada App Password istifadə olunur
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
