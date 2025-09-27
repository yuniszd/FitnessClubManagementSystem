namespace FCMS.Application.Events;

public class SubscriptionExpiringEvent
{
    public string Email { get; set; } = default!;
    public int RemainingVisits { get; set; }
}
