namespace FCMS.Persistence.BackgroundJobs;

public class SubscriptionReminderEvent
{
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PlanName { get; set; } = null!;
    public int? RemainingVisits { get; set; }    // optional
    public int DaysLeft { get; set; }            // qalan gün sayı
}

