namespace FCMS.Application.Abstracts;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, byte[]? attachment = null);
}
