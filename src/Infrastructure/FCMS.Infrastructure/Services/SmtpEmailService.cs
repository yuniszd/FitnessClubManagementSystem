using System.Net.Mail;
using System.Net;
using FCMS.Application.Abstracts;
using FCMS.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace FCMS.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_settings.Server, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = true
        };

        var mail = new MailMessage(_settings.Username, to, subject, body);
        await client.SendMailAsync(mail);
    }
}