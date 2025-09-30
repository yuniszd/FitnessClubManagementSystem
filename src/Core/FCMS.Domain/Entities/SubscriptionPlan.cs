namespace FCMS.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } // Kiçik zal, Orta zal, Böyük zal
    public int DurationInMonths { get; set; }
    public decimal Price { get; set; }

    // Default allowed visits
    public int DefaultVisits { get; set; } = 10;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

