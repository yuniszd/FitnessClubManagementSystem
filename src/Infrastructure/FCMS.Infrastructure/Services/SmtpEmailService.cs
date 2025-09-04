using System.Net.Mail;
using System.Net;
using FCMS.Application.Abstracts;

namespace FCMS.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;

    public SmtpEmailService(string host,
                            int port,
                            string fromEmail,
                            string password)
    {
        _fromEmail = fromEmail;
        _smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
            return;

        var mail = new MailMessage(_fromEmail, to, subject, body)
        {
            IsBodyHtml = true
        };
        await _smtpClient.SendMailAsync(mail);
    }
}
