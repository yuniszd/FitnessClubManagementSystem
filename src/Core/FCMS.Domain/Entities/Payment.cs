namespace FCMS.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid SubscriptionId { get; set; }   // FK to Subscription
    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; }
    public Subscription Subscription { get; set; }  // navigation property
}