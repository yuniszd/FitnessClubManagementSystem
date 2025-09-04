namespace FCMS.Domain.Entities;

public class Member : BaseEntity
{
    public string FullName { get; set; }
    public string CardNumber { get; set; }    // QR / giriş kodu

    public string? PhoneNumber { get; set; }  // optional
    public string? Email { get; set; }        // optional
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<CheckInLog> CheckInLogs { get; set; } = new List<CheckInLog>();
}
