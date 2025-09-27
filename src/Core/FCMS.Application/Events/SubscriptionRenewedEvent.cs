namespace FCMS.Application.Events;
public class SubscriptionRenewedEvent
{
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string PlanName { get; set; } = default!;
    public DateTime NewEndDate { get; set; }
}