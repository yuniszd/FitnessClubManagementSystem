namespace FCMS.Domain.Entities;

public class Subscription : BaseEntity
{
        public Guid MemberId { get; set; }                       // FK to Member
        public Guid SubscriptionPlanId { get; set; }            // FK to Plan

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Member Member { get; set; }                      // navigation property
        public SubscriptionPlan SubscriptionPlan { get; set; }  // navigation property

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();  // initialize collection

        public int? AllowedVisits { get; set; } // null = limitsiz
        public int UsedVisits { get; set; }
        public bool IsActive => DateTime.UtcNow <= EndDate && (AllowedVisits == null || UsedVisits < AllowedVisits);


}
