namespace FCMS.Domain.Entities;

public class Member : BaseEntity
{
    public string FullName { get; set; }
    public string CardNumber { get; set; }    

    public string? PhoneNumber { get; set; }  
    public string? Email { get; set; }        
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false; 

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<CheckInLog> CheckInLogs { get; set; } = new List<CheckInLog>();
}
