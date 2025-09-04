namespace FCMS.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } // Kiçik zal, Orta zal, Böyük zal
    public int DurationInMonths { get; set; }
    public decimal Price { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
