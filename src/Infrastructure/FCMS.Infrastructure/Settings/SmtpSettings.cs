namespace FCMS.Infrastructure.Settings;

public class SmtpSettings
{
    public string Server { get; set; } = default!;
    public int Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool EnableSsl { get; set; } = true;
}
